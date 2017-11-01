import * as React from 'react';
import { Link, RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { ApplicationState }  from '../store';
import * as JobsState from '../store/Jobs';

// At runtime, Redux will merge together...
type JobProps =
    JobsState.JobsState        // ... state we've requested from the Redux store
    & typeof JobsState.actionCreators      // ... plus action creators we've requested
    & RouteComponentProps<{ jobId: string }>;

class Job extends React.Component<JobProps, {}> {
    componentWillMount() {
        // This method runs when the component is first added to the page
        let jobId = this.props.match.params.jobId || "";

        this.props.requestJob(jobId);
    }

    componentWillReceiveProps(nextProps: JobProps) {
        // This method runs when incoming props (e.g., route params) change
        let jobId = nextProps.currentJob.id || "";
        
        if(jobId !== this.props.currentJob.id && jobId !== "")
        {
            const location = { pathname: `/jobdetail/${jobId}` };

            nextProps.history.push(location);
        }
    }

    handleChange = (event: any) => { //fat arrow syntax
        const e = event.target as HTMLInputElement;
        this.props.updateCurrentJob(e.name, e.value);
    }

    saveJob = (event: any) => {
        this.props.saveJob();
    }

    public render() {
        return <div>
            <h1>
                { this.renderBreadcrumb() }
            </h1>
            { this.renderJobDetailForm() }
        </div>;
    }

    private renderJobDetailForm() {
        return <form className="form-horizontal">
          <div className="form-group">
            <label htmlFor="inputName" className="col-sm-2 control-label">Name</label>
            <div className="col-sm-10">
              <input type="text" className="form-control" id="inputName" name="name" placeholder="Name" onChange={this.handleChange} value={this.props.currentJob.name} />
            </div>
          </div>
          <div className="form-group">
            <div className="col-sm-offset-2 col-sm-10">
              <button type="button" className="btn btn-default" onClick={ this.saveJob }>Save</button>
            </div>
          </div>
        </form>;
    }

    private renderBreadcrumb()
    {
        let startDateIndex = (this.props.startDateIndex || 0);

        return <ol className="breadcrumb">
            <li>
                <Link to={`/jobs/${startDateIndex}`}>Jobs</Link>
            </li>
            {this.props.currentJob.id === ""
                ? <li className="active">New Job</li>
                : <li className="active">{this.props.currentJob.name}</li>
            }
        </ol>;
    }
}

export default connect(
    (state: ApplicationState) => state.jobs, // Selects which state properties are merged into the component's props
    JobsState.actionCreators                 // Selects which action creators are merged into the component's props
)(Job) as typeof Job;
