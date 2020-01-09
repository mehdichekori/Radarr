import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Popover from 'Components/Tooltip/Popover';
import MovieQuality from 'Movie/MovieQuality';
import MovieFormats from 'Movie/MovieFormats';
import MovieLanguage from 'Movie/MovieLanguage';
import HistoryDetailsConnector from 'Activity/History/Details/HistoryDetailsConnector';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import styles from './MovieHistoryRow.css';

function getTitle(eventType) {
  switch (eventType) {
    case 'grabbed': return 'Grabbed';
    case 'seriesFolderImported': return 'Series Folder Imported';
    case 'downloadFolderImported': return 'Download Folder Imported';
    case 'downloadFailed': return 'Download Failed';
    case 'episodeFileDeleted': return 'Episode File Deleted';
    case 'movieFileDeleted': return 'Movie File Deleted';
    case 'movieFolderImported': return 'Movie Folder Imported';
    default: return 'Unknown';
  }
}

class MovieHistoryRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isMarkAsFailedModalOpen: false
    };
  }

  //
  // Listeners

  onMarkAsFailedPress = () => {
    this.setState({ isMarkAsFailedModalOpen: true });
  }

  onConfirmMarkAsFailed = () => {
    this.props.onMarkAsFailedPress(this.props.id);
    this.setState({ isMarkAsFailedModalOpen: false });
  }

  onMarkAsFailedModalClose = () => {
    this.setState({ isMarkAsFailedModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      eventType,
      sourceTitle,
      quality,
      customFormats,
      languages,
      qualityCutoffNotMet,
      date,
      data
    } = this.props;

    const {
      isMarkAsFailedModalOpen
    } = this.state;

    return (
      <TableRow>
        <HistoryEventTypeCell
          eventType={eventType}
          data={data}
        />

        <TableRowCell>
          {sourceTitle}
        </TableRowCell>

        <TableRowCell>
          <MovieLanguage
            languages={languages}
          />
        </TableRowCell>

        <TableRowCell>
          <MovieQuality
            quality={quality}
            isCutoffNotMet={qualityCutoffNotMet}
          />
        </TableRowCell>

        <TableRowCell key={name}>
          <MovieFormats
            formats={customFormats}
          />
        </TableRowCell>

        <RelativeDateCellConnector
          date={date}
        />

        <TableRowCell className={styles.details}>
          <Popover
            anchor={
              <Icon
                name={icons.INFO}
              />
            }
            title={getTitle(eventType)}
            body={
              <HistoryDetailsConnector
                eventType={eventType}
                sourceTitle={sourceTitle}
                data={data}
              />
            }
            position={tooltipPositions.LEFT}
          />
        </TableRowCell>

        <TableRowCell className={styles.actions}>
          {
            eventType === 'grabbed' &&
              <IconButton
                title="Mark as failed"
                name={icons.REMOVE}
                onPress={this.onMarkAsFailedPress}
              />
          }
        </TableRowCell>

        <ConfirmModal
          isOpen={isMarkAsFailedModalOpen}
          kind={kinds.DANGER}
          title="Mark as Failed"
          message={`Are you sure you want to mark '${sourceTitle}' as failed?`}
          confirmLabel="Mark as Failed"
          onConfirm={this.onConfirmMarkAsFailed}
          onCancel={this.onMarkAsFailedModalClose}
        />
      </TableRow>
    );
  }
}

MovieHistoryRow.propTypes = {
  id: PropTypes.number.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object).isRequired,
  quality: PropTypes.object.isRequired,
  customFormats: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  movie: PropTypes.object.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default MovieHistoryRow;
