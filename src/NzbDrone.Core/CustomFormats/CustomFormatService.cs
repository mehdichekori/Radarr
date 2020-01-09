﻿using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.CustomFormats.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatService
    {
        void Update(CustomFormat customFormat);
        CustomFormat Insert(CustomFormat customFormat);
        List<CustomFormat> All();
        CustomFormat GetById(int id);
        void Delete(int id);
    }

    public class CustomFormatService : ICustomFormatService
    {
        public static Dictionary<int, CustomFormat> AllCustomFormats;

        private readonly ICustomFormatRepository _formatRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICached<Dictionary<int, CustomFormat>> _cache;

        public CustomFormatService(ICustomFormatRepository formatRepository,
                                   ICacheManager cacheManager,
                                   IEventAggregator eventAggregator)
        {
            _formatRepository = formatRepository;
            _eventAggregator = eventAggregator;
            _cache = cacheManager.GetCache<Dictionary<int, CustomFormat>>(typeof(CustomFormat), "formats");

            // Fill up the cache for subsequent DB lookups
            All();
        }

        public void Update(CustomFormat customFormat)
        {
            _formatRepository.Update(customFormat);
            _cache.Clear();
        }

        public CustomFormat Insert(CustomFormat customFormat)
        {
            var result = _formatRepository.Insert(customFormat);

            _cache.Clear();
            _eventAggregator.PublishEvent(new CustomFormatAddedEvent(result));

            return result;
        }

        public void Delete(int id)
        {
            var format = _formatRepository.Get(id);

            _formatRepository.Delete(id);

            _cache.Clear();

            _eventAggregator.PublishEvent(new CustomFormatDeletedEvent(format));
        }

        private Dictionary<int, CustomFormat> AllDictionary()
        {
            return _cache.Get("all", () =>
            {
                var all = _formatRepository.All().Select(x => (CustomFormat)x).ToDictionary(m => m.Id);
                AllCustomFormats = all;
                return all;
            });
        }

        public List<CustomFormat> All()
        {
            return AllDictionary().Values.ToList();
        }

        public CustomFormat GetById(int id)
        {
            return AllDictionary()[id];
        }

        public static Dictionary<string, List<CustomFormat>> Templates => new Dictionary<string, List<CustomFormat>>
                {
                    {
                        "Easy", new List<CustomFormat>
                        {
                            new CustomFormat("x264", @"C_RX_(x|h)\.?264"),
                            new CustomFormat("x265", @"C_RX_(((x|h)\.?265)|(HEVC))"),
                            new CustomFormat("Simple Hardcoded Subs", "C_RX_subs?"),
                            new CustomFormat("Multi Language", "L_English", "L_French")
                        }
                    },
                    {
                        "Intermediate", new List<CustomFormat>
                        {
                            new CustomFormat("Hardcoded Subs", @"C_RX_\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC|SUBBED))\b"),
                            new CustomFormat("Surround", @"C_RX_\b((7|5).1)\b"),
                            new CustomFormat("Preferred Words", @"C_RX_\b(SPARKS|Framestor)\b"),
                            new CustomFormat("Scene", @"I_G_Scene"),
                            new CustomFormat("Internal Releases", @"I_HDB_Internal", @"I_AHD_Internal")
                        }
                    },
                    {
                        "Advanced", new List<CustomFormat>
                        {
                            new CustomFormat("Custom")
                        }
                    }
                };
    }
}
