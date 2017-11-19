import * as React from 'react';
import { Link, RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { ApplicationState }  from '../store';
import * as JobsState from '../store/Jobs';

// At runtime, Redux will merge together...
type JobsProps =
    JobsState.JobsState        // ... state we've requested from the Redux store
    & typeof JobsState.actionCreators      // ... plus action creators we've requested
    & RouteComponentProps<{ startDateIndex: string }>; // ... plus incoming routing parameters

class Jobs extends React.Component<JobsProps, {}> {
    componentWillMount() {
        // This method runs when the component is first added to the page
        let startDateIndex = parseInt(this.props.match.params.startDateIndex) || 0;
        this.props.requestJobs(startDateIndex);
    }

    componentWillReceiveProps(nextProps: JobsProps) {
        // This method runs when incoming props (e.g., route params) change
        let startDateIndex = parseInt(nextProps.match.params.startDateIndex) || 0;
        this.props.requestJobs(startDateIndex);
    }

    handleDelete = (jobId: string) =>
    { //fat arrow syntax
        this.props.deleteJob(jobId);
    }

    public render() {
        return <div>
            <h1>
                <ol className="breadcrumb">
                    <li className="active">
                        Jobs
                    </li>
                </ol>
            </h1>
            { this.renderForecastsTable() }
            { this.renderPagination() }
        </div>;
    }

    private renderForecastsTable() {
        return <table className='table'>
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Date Created</th>
                    <th>Date Modified</th>
                    <th>Created By</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody>
            {this.props.jobs.map(job =>
                <tr key={ job.id }>
                    <td>{ job.name }</td>
                    <td>{ job.dateCreated }</td>
                    <td>{ job.dateModified }</td>
                    <td>{ job.createdBy.email }</td>
                    <td>
                        <Link className='btn btn-default pull-right' to={ `/jobdetail/${job.id}` }>Edit</Link>
                        <button type="button" className="btn btn-default pull-right" onClick={ () => this.handleDelete(job.id)}>Delete</button>
                    </td>
                </tr>
            )}
            </tbody>
        </table>;
    }

    private renderPagination() {
        let prevStartDateIndex = (this.props.startDateIndex || 0) - 1;
        let nextStartDateIndex = (this.props.startDateIndex || 0) + 1;
        let isPrevDisabled = prevStartDateIndex < 0;

        return <p className='clearfix text-center'>
            { isPrevDisabled
                ? <Link className='btn btn-default pull-left' to={ `/jobs/` }>Previous</Link>
                : <Link className='btn btn-default pull-left' to={ `/jobs/${ prevStartDateIndex }` }>Previous</Link>
            }
            <Link className='btn btn-default pull-right' to={ `/jobs/${ nextStartDateIndex }` }>Next</Link>
            { this.props.isLoading ? <span>Loading...</span> : [] }
        </p>;
    }
}

export default connect(
    (state: ApplicationState) => state.jobs, // Selects which state properties are merged into the component's props
    JobsState.actionCreators                 // Selects which action creators are merged into the component's props
)(Jobs) as typeof Jobs;
