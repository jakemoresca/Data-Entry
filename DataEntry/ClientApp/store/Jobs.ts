import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import * as jQuery from 'jquery';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface JobsState {
    isLoading: boolean;
    startDateIndex?: number;
    jobs: Job[];
    currentJob: Job;
}

export interface Job {
    id: string;
    name: string;
    dateCreated: string;
    dateModified: string;
    createdBy: string;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestJobsAction {
    type: 'REQUEST_JOBS';
    startDateIndex: number;
}

interface ReceiveJobsAction {
    type: 'RECEIVE_JOBS';
    startDateIndex: number;
    jobs: Job[];
}

interface RequestJobAction {
    type: 'REQUEST_JOB';
    jobId: string;
}

interface ReceiveJobAction {
    type: 'RECEIVE_JOB';
    jobId: string;
    job: Job;
}

interface NewJobAction {
    type: "NEW_JOB";
}

interface UpdateCurrentJobValues {
    type: "UPDATE_CURRENT_JOB_VALUE";
    field: string;
    value: string;
}

interface SaveJobAction {
    type: "SAVE_JOB";
    currentJob: Job;
}

interface DeleteJobAction {
    type: "DELETE_JOB";
}

interface DeleteJobSuccessAction {
    type: "DELETE_JOB_SUCCESS";
    currentJob: Job;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestJobsAction | ReceiveJobsAction | RequestJobAction | ReceiveJobAction | UpdateCurrentJobValues | NewJobAction | SaveJobAction | DeleteJobAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    requestJobs: (startDateIndex: number): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        if (startDateIndex !== getState().jobs.startDateIndex) {
            let fetchTask = fetch(`api/job/Jobs?startDateIndex=${ startDateIndex }`)
                .then(response => response.json() as Promise<Job[]>)
                .then(data => {
                    dispatch({ type: 'RECEIVE_JOBS', startDateIndex: startDateIndex, jobs: data });
                });

            addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'REQUEST_JOBS', startDateIndex: startDateIndex });
        }
    },

    requestJob: (jobId: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        if(jobId !== "")
        {
            let fetchTask = jQuery.get(`api/job/${ jobId }`)
                .then(response => response as Promise<Job>)
                .then(data => {
                    dispatch({ type: 'RECEIVE_JOB', jobId: jobId, job: data });
                });

            addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'REQUEST_JOB', jobId: jobId });
        }
        else
        {
            dispatch({ type: 'NEW_JOB' });
        }
    },
    
    updateCurrentJob: (field: string, value: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        dispatch({ type: 'UPDATE_CURRENT_JOB_VALUE', field: field, value: value });
    },

    saveJob: (): AppThunkAction<KnownAction> => (dispatch, getState) =>
    {
        if (getState().jobs.currentJob.id === "")
        {
            let insertTask = jQuery.post("api/job", { job: getState().jobs.currentJob })
                .then(response => response as Promise<Job>)
                .then(data =>
                {
                    dispatch({ type: "RECEIVE_JOB", job: data, jobId: data.id });
                });

            addTask(insertTask);
        }
        else
        {
            let updateTask = jQuery.ajax({
                    url: `api/job/${getState().jobs.currentJob.id}`,
                    type: "PUT",
                    data: { job: getState().jobs.currentJob }
                })
                .then(response => response as Promise<Job>)
                .then(data =>
                {
                    dispatch({ type: "RECEIVE_JOB", job: data, jobId: data.id });
                });

            addTask(updateTask);
        }
        
        dispatch({ type: "SAVE_JOB", currentJob: getState().jobs.currentJob });
    },

    deleteJob: (jobId: string): AppThunkAction<KnownAction> => (dispatch, getState) =>
    {
        if (jobId !== "")
        {
            let deleteTask = jQuery.ajax({
                    url: `api/job/${jobId}`,
                    type: "DELETE"
                })
                .then(response =>
                {
                    let startDateIndex = getState().jobs.startDateIndex || 0;
                    let fetchTask = fetch(`api/job/Jobs?startDateIndex=${startDateIndex}`)
                        .then(response => response.json() as Promise<Job[]>)
                        .then(data =>
                        {
                            dispatch({ type: 'RECEIVE_JOBS', startDateIndex: startDateIndex, jobs: data });
                        });

                    addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
                    dispatch({ type: 'REQUEST_JOBS', startDateIndex: startDateIndex });
                });

            addTask(deleteTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'DELETE_JOB' });
        }
    },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: JobsState = { 
    jobs: [], 
    isLoading: false, 
    currentJob: {
        id: "",
        name: "",
        dateCreated: "",
        dateModified: "",
        createdBy: ""
    }
};

export const reducer: Reducer<JobsState> = (state: JobsState, incomingAction: Action) => {
    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQUEST_JOBS':
            return {
                ...state,
                startDateIndex: action.startDateIndex,
                jobs: state.jobs,
                isLoading: true,
                currentJob: {
                    ...state.currentJob,
                    id: "",
                    name: "",
                    dateCreated: "",
                    dateModified: "",
                    createdBy: ""
                }
            };
        case 'RECEIVE_JOBS':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            if (action.startDateIndex === state.startDateIndex) {
                return {
                    ...state,
                    startDateIndex: action.startDateIndex,
                    jobs: action.jobs,
                    isLoading: false,
                    currentJob: {
                        ...state.currentJob,
                        id: "",
                        name: "",
                        dateCreated: "",
                        dateModified: "",
                        createdBy: ""
                    }
                };
            }
            break;
        case 'REQUEST_JOB':
            return {
                ...state,
                startDateIndex: state.startDateIndex,
                jobs: state.jobs,
                isLoading: true,
                currentJob: {
                    ...state.currentJob,
                    id: "",
                    name: "",
                    dateCreated: "",
                    dateModified: "",
                    createdBy: ""
                }
            };
        case 'RECEIVE_JOB':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            return {
                ...state,
                startDateIndex: state.startDateIndex,
                jobs: state.jobs,
                isLoading: false,
                currentJob: action.job
            };
        case "UPDATE_CURRENT_JOB_VALUE":
            return {
                ...state,
                startDateIndex: 0,
                jobs: state.jobs,
                isLoading: false,
                currentJob: {
                    ...state.currentJob,
                    [action.field]: action.value
                }
            };
        case "NEW_JOB":
            return {
                ...state,
                startDateIndex: 0,
                jobs: state.jobs,
                isLoading: false,
                currentJob: {
                    ...state.currentJob,
                    id: "",
                    name: "",
                    dateCreated: "",
                    dateModified: "",
                    createdBy: ""
                }
            };
        case "SAVE_JOB":
            return {
                jobs: state.jobs,
                isLoading: true,
                currentJob: state.currentJob
            };
        case "DELETE_JOB":
            return {
                ...state,
                startDateIndex: 0,
                jobs: state.jobs,
                isLoading: false,
                currentJob: {
                    ...state.currentJob,
                    id: "",
                    name: "",
                    dateCreated: "",
                    dateModified: "",
                    createdBy: ""
                }
            }
        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};