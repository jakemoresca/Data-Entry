import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import * as jQuery from 'jquery';
import { callApi } from './callApi';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface UsersState {
    isLoading: boolean;
    startDateIndex?: number;
    users: User[];
    currentUser: User;
}

export interface User
{
    id: string;
    userName: string;
    normalizedUserName: string;
    email: string;
    normalizedEmail: string;
    emailConfirmed: boolean;
    passwordHash: string;
    securityStamp: string;
    concurrencyStamp: string;
    phoneNumber: string;
    phoneNumberConfirmed: boolean;
    twoFactorEnabled: boolean;
    lockoutEnd?: string;
    lockoutEnabled: boolean;
    accessFailedCount: number;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestUsersAction {
    type: 'REQUEST_USERS';
    startDateIndex: number;
}

interface ReceiveUsersAction {
    type: 'RECEIVE_USERS';
    startDateIndex: number;
    users: User[];
}

interface RequestUserAction {
    type: 'REQUEST_USER';
    userId: string;
}

interface ReceiveUserAction {
    type: 'RECEIVE_USER';
    userId: string;
    user: User;
}

interface NewUserAction {
    type: "NEW_USER";
}

interface UpdateCurrentUserValues {
    type: "UPDATE_CURRENT_USER_VALUE";
    field: string;
    value: string;
}

interface SaveUserAction {
    type: "SAVE_USER";
    currentUser: User;
}

interface DeleteUserAction {
    type: "DELETE_USER";
}

interface DeleteUserSuccessAction {
    type: "DELETE_USER_SUCCESS";
    currentUser: User;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestUsersAction | ReceiveUsersAction | RequestUserAction | ReceiveUserAction | UpdateCurrentUserValues | NewUserAction | SaveUserAction | DeleteUserAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    requestUsers: (startDateIndex: number): AppThunkAction<KnownAction> => (dispatch, getState) =>
    {
        //Only load data if it's something we don't already have (and are not already loading)
        if (startDateIndex !== getState().users.startDateIndex) {
            let fetchTask = callApi(`api/user/Users?startDateIndex=${startDateIndex}`, true) //fetch(`api/user/Users?startDateIndex=${ startDateIndex }`)
                .then(response => response as Promise<User[]>)
                .then(data =>
                {
                    dispatch({ type: 'RECEIVE_USERS', startDateIndex: startDateIndex, users: data });
                });

            addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'REQUEST_USERS', startDateIndex: startDateIndex });
        }
    },

    requestUser: (userId: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        if(userId !== "")
        {
            let fetchTask = callApi(`api/user/${userId}`, true)// jQuery.get(`api/user/${ userId }`)
                .then(response => response as Promise<User>)
                .then(data => {
                    dispatch({ type: 'RECEIVE_USER', userId: userId, user: data });
                });

            addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'REQUEST_USER', userId: userId });
        }
        else
        {
            dispatch({ type: 'NEW_USER' });
        }
    },
    
    updateCurrentUser: (field: string, value: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        dispatch({ type: 'UPDATE_CURRENT_USER_VALUE', field: field, value: value });
    },

    saveUser: (): AppThunkAction<KnownAction> => (dispatch, getState) =>
    {
        if (getState().users.currentUser.id === "")
        {
            var data = {
                type: 'POST',
                data: { user: getState().users.currentUser }
            };

            let insertTask = callApi("api/user", true, data) //jQuery.post("api/user", { user: getState().users.currentUser })
                .then(response => response as Promise<User>)
                .then(data =>
                {
                    dispatch({ type: "RECEIVE_USER", user: data, userId: data.id });
                });

            addTask(insertTask);
        }
        else
        {
            var data = {
                type: 'PUT',
                data: { user: getState().users.currentUser }
            };

            let updateTask = callApi(`api/user/${getState().users.currentUser.id}`, true, data) // jQuery.ajax({
                    //url: `api/user/${getState().users.currentUser.id}`,
                   // type: "PUT",
                   // data: { user: getState().users.currentUser }
                    //})
                .then(response => response as Promise<User>)
                .then(data =>
                {
                    dispatch({ type: "RECEIVE_USER", user: data, userId: data.id });
                });

            addTask(updateTask);
        }
        
        dispatch({ type: "SAVE_USER", currentUser: getState().users.currentUser });
    },

    deleteUser: (userId: string): AppThunkAction<KnownAction> => (dispatch, getState) =>
    {
        if (userId !== "")
        {
            var data = {
                type: 'DELETE'
            };

            let deleteTask = callApi(`api/user/${userId}`, true, data) //jQuery.ajax({ url: `api/user/${userId}`, type: "DELETE" })
                .then(response =>
                {
                    let startDateIndex = getState().users.startDateIndex || 0;
                    let fetchTask = callApi(`api/user/Users?startDateIndex=${startDateIndex}`, true) //fetch(`api/user/Users?startDateIndex=${startDateIndex}`)
                        .then(response => response as Promise<User[]>)
                        .then(data =>
                        {
                            dispatch({ type: 'RECEIVE_USERS', startDateIndex: startDateIndex, users: data });
                        });

                    addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
                    dispatch({ type: 'REQUEST_USERS', startDateIndex: startDateIndex });
                });

            addTask(deleteTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'DELETE_USER' });
        }
    },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: UsersState = { 
    users: [], 
    isLoading: false, 
    currentUser: {
        id: "",
        userName: "",
        normalizedUserName: "",
        email: "",
        normalizedEmail: "",
        emailConfirmed: false,
        passwordHash: "",
        securityStamp: "",
        concurrencyStamp: "",
        phoneNumber: "",
        phoneNumberConfirmed: false,
        twoFactorEnabled: false,
        lockoutEnd: "",
        lockoutEnabled: false,
        accessFailedCount: 0
    }
};

export const reducer: Reducer<UsersState> = (state: UsersState, incomingAction: Action) => {
    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQUEST_USERS':
            return {
                ...state,
                startDateIndex: action.startDateIndex,
                users: state.users,
                isLoading: true,
                currentUser: {
                    ...state.currentUser,
                    id: "",
                    userName: "",
                    normalizedUserName: "",
                    email: "",
                    normalizedEmail: "",
                    emailConfirmed: false,
                    passwordHash: "",
                    securityStamp: "",
                    concurrencyStamp: "",
                    phoneNumber: "",
                    phoneNumberConfirmed: false,
                    twoFactorEnabled: false,
                    lockoutEnd: "",
                    lockoutEnabled: false,
                    accessFailedCount: 0
                }
            };
        case 'RECEIVE_USERS':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            if (action.startDateIndex === state.startDateIndex) {
                return {
                    ...state,
                    startDateIndex: action.startDateIndex,
                    users: action.users,
                    isLoading: false,
                    currentUser: {
                        ...state.currentUser,
                        id: "",
                        userName: "",
                        normalizedUserName: "",
                        email: "",
                        normalizedEmail: "",
                        emailConfirmed: false,
                        passwordHash: "",
                        securityStamp: "",
                        concurrencyStamp: "",
                        phoneNumber: "",
                        phoneNumberConfirmed: false,
                        twoFactorEnabled: false,
                        lockoutEnd: "",
                        lockoutEnabled: false,
                        accessFailedCount: 0
                    }
                };
            }
            break;
        case 'REQUEST_USER':
            return {
                ...state,
                startDateIndex: state.startDateIndex,
                users: state.users,
                isLoading: true,
                currentUser: {
                    ...state.currentUser,
                    id: "",
                    userName: "",
                    normalizedUserName: "",
                    email: "",
                    normalizedEmail: "",
                    emailConfirmed: false,
                    passwordHash: "",
                    securityStamp: "",
                    concurrencyStamp: "",
                    phoneNumber: "",
                    phoneNumberConfirmed: false,
                    twoFactorEnabled: false,
                    lockoutEnd: "",
                    lockoutEnabled: false,
                    accessFailedCount: 0
                }
            };
        case 'RECEIVE_USER':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            return {
                ...state,
                startDateIndex: state.startDateIndex,
                users: state.users,
                isLoading: false,
                currentUser: action.user
            };
        case "UPDATE_CURRENT_USER_VALUE":
            return {
                ...state,
                startDateIndex: 0,
                users: state.users,
                isLoading: false,
                currentUser: {
                    ...state.currentUser,
                    [action.field]: action.value
                }
            };
        case "NEW_USER":
            return {
                ...state,
                startDateIndex: 0,
                users: state.users,
                isLoading: false,
                currentUser: {
                    ...state.currentUser,
                    id: "",
                    userName: "",
                    normalizedUserName: "",
                    email: "",
                    normalizedEmail: "",
                    emailConfirmed: false,
                    passwordHash: "",
                    securityStamp: "",
                    concurrencyStamp: "",
                    phoneNumber: "",
                    phoneNumberConfirmed: false,
                    twoFactorEnabled: false,
                    lockoutEnd: "",
                    lockoutEnabled: false,
                    accessFailedCount: 0
                }
            };
        case "SAVE_USER":
            return {
                users: state.users,
                isLoading: true,
                currentUser: state.currentUser
            };
        case "DELETE_USER":
            return {
                ...state,
                startDateIndex: 0,
                users: state.users,
                isLoading: false,
                currentUser: {
                    ...state.currentUser,
                    id: "",
                    userName: "",
                    normalizedUserName: "",
                    email: "",
                    normalizedEmail: "",
                    emailConfirmed: false,
                    passwordHash: "",
                    securityStamp: "",
                    concurrencyStamp: "",
                    phoneNumber: "",
                    phoneNumberConfirmed: false,
                    twoFactorEnabled: false,
                    lockoutEnd: "",
                    lockoutEnabled: false,
                    accessFailedCount: 0
                }
            }
        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};