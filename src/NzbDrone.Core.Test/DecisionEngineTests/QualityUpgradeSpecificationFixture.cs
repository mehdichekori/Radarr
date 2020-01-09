﻿using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityUpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        public static object[] IsUpgradeTestCases =
        {
            new object[] { Quality.SDTV, 1, Quality.SDTV, 2, Quality.SDTV, true },
            new object[] { Quality.WEBDL720p, 1, Quality.WEBDL720p, 2, Quality.WEBDL720p, true },
            new object[] { Quality.SDTV, 1, Quality.SDTV, 1, Quality.SDTV, false },
            new object[] { Quality.WEBDL720p, 1, Quality.HDTV720p, 2, Quality.Bluray720p, false },
            new object[] { Quality.WEBDL720p, 1, Quality.HDTV720p, 2, Quality.WEBDL720p, false },
            new object[] { Quality.WEBDL720p, 1, Quality.WEBDL720p, 1, Quality.WEBDL720p, false },
            new object[] { Quality.WEBDL1080p, 1, Quality.WEBDL1080p, 1, Quality.WEBDL1080p, false }
        };

        [SetUp]
        public void Setup()
        {
        }

        private void GivenAutoDownloadPropers(bool autoDownloadPropers)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoDownloadPropers)
                  .Returns(autoDownloadPropers);
        }

        [Test]
        [TestCaseSource("IsUpgradeTestCases")]
        public void IsUpgradeTest(Quality current, int currentVersion, Quality newQuality, int newVersion, Quality cutoff, bool expected)
        {
            GivenAutoDownloadPropers(true);

            var profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            Subject.IsUpgradable(profile,
                                 new QualityModel(current, new Revision(version: currentVersion)),
                                 new List<CustomFormat>(),
                                 new QualityModel(newQuality, new Revision(version: newVersion)),
                                 new List<CustomFormat>())
                    .Should().Be(expected);
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_false()
        {
            GivenAutoDownloadPropers(false);

            var profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() };

            Subject.IsUpgradable(profile,
                                 new QualityModel(Quality.DVD, new Revision(version: 2)),
                                 new List<CustomFormat>(),
                                 new QualityModel(Quality.DVD, new Revision(version: 1)),
                                 new List<CustomFormat>())
                    .Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_release_and_existing_file_are_the_same()
        {
            var profile = new Profile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>())
                   .Should().BeFalse();
        }
    }
}
