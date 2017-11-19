import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import * as jQuery from 'jquery';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface AccountState {
    isLoading: boolean;
    isLoggedIn: boolean;
    currentAccount: Account;
}

export interface Account {
    id: string;
    name: string;
    username: string;
    password: string;
    access_token: Token;
}

export interface Token
{
    access_token: string;
    expires_in: number;
    token_type: string;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestAccountAction {
    type: 'REQUEST_ACCOUNT';
}

interface ReceiveAccountAction {
    type: 'RECEIVE_ACCOUNT';
    isLoggedIn: boolean;
    currentAccount: Account;
}

interface LoginAction {
    type: 'LOGIN';
    isLoggedIn: boolean;
}

interface LogoutAction {
    type: 'LOGOUT';
}

interface UpdateCurrentLoginValueAction
{
    type: "UPDATE_CURRENT_LOGIN_VALUE";
    field: string;
    value: string;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestAccountAction | ReceiveAccountAction | LoginAction | LogoutAction | UpdateCurrentLoginValueAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    login: (username: string, password: string): AppThunkAction<KnownAction> => (dispatch, getState) => {

        let loginJSON: any = {
            "grant_type": "password",
            "client_id": "js",
            "client_secret": "secret",
            "username": username,
            "password": password,
            "scope": "openid"
        };

        var formBody = [];
        for (var property in loginJSON)
        {
            var encodedKey = encodeURIComponent(property);
            var encodedValue = encodeURIComponent(loginJSON[property]);
            formBody.push(encodedKey + "=" + encodedValue);
        }

        var loginBody = formBody.join("&");

        // Only load data if it's something we don't already have (and are not already loading)
        let fetchTask = fetch('/connect/token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: loginBody
        })
        .then(response => response.json() as Promise<any>)
        .then(data =>
        {
            localStorage.setItem('access_token', data.access_token)

            var account = {
                id: "",
                name: "",
                username: username,
                password: "",
                access_token: data
            };

            dispatch({ type: 'RECEIVE_ACCOUNT', isLoggedIn: true, currentAccount: account });
        });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: 'LOGIN', isLoggedIn: false });

    },

    updateCurrentLogin: (field: string, value: string): AppThunkAction<KnownAction> => (dispatch, getState) =>
    {
        dispatch({ type: 'UPDATE_CURRENT_LOGIN_VALUE', field: field, value: value });
    },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: AccountState = { 
    isLoading: false,
    isLoggedIn: false,
    currentAccount: {
        id: "",
        name: "",
        username: "",
        password: "",
        access_token: {
            access_token: "",
            expires_in: 0,
            token_type: ""
        }
    }
};

export const reducer: Reducer<AccountState> = (state: AccountState, incomingAction: Action) => {
    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'LOGIN':
        case 'REQUEST_ACCOUNT':
        case 'LOGOUT':
            return {
                ...state,
                isLoading: false,
                isLoggedIn: localStorage.getItem('access_token') ? true : false,
                currentAccount: {
                    ...state.currentAccount,
                    id: "",
                    name: "",
                    username: "",
                    password: "",
                    access_token: {
                        ...state.currentAccount.access_token,
                        access_token: "",
                        expires_in: 0,
                        token_type: ""
                    }
                }
            };
        case 'RECEIVE_ACCOUNT':
            return {
                ...state,
                isLoading: false,
                isLoggedIn: localStorage.getItem('access_token') ? true : false,
                currentAccount: {
                    ...state.currentAccount,
                    id: "",
                    //name: "",
                    //username: action.currentAccount.username,
                    password: "",
                    access_token: {
                        ...state.currentAccount.access_token,
                        access_token: action.currentAccount.access_token.access_token,
                        expires_in: action.currentAccount.access_token.expires_in,
                        token_type: action.currentAccount.access_token.token_type
                    }
                }
            };
        case "UPDATE_CURRENT_LOGIN_VALUE":
            return {
                ...state,
                isLoading: false,
                isLoggedIn: true,
                currentAccount: {
                    ...state.currentAccount,
                    [action.field]: action.value
                }
            };
        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};