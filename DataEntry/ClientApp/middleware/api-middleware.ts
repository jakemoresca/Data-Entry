import { Middleware, MiddlewareAPI, Dispatch, Action } from "redux";
import { ApplicationState } from '../store';

const BASE_URL = 'api/';

export interface ExtendedAPIMiddleware<StateType> extends Middleware {
    <S extends StateType>(api: MiddlewareAPI<S>): (next: Dispatch<S>) => Dispatch<S>;
}
 
export const loggerMiddleware: ExtendedAPIMiddleware<ApplicationState> = <S extends ApplicationState>(api: MiddlewareAPI<S>) =>
    (next: Dispatch<S>) =>
        <A extends Action>(action: A): A =>
        {
            if (action.type == "CALL_API")
            {

            }
            console.log(`Before ${action.type}`);
            console.log(api.getState())
            const result = next(action);
            console.log(`After ${action.type}`); // Can use: api.getState()
            return result;
        };

function callApi(endpoint: any, authenticated: any)
{
    let token = localStorage.getItem('access_token') || null
    let config = {}

    if (authenticated)
    {
        if (token)
        {
            config = {
                headers: { 'Authorization': `Bearer ${token}` }
            }
        }
        else
        {
            throw "No token saved!"
        }
    }

    return fetch(endpoint, config)
        .then(response =>
            response.text().then(text => ({ text, response }))
        ).then(({ text, response }: any) =>
        {
            if (!response.ok)
            {
                return Promise.reject(text)
            }

            return text
        }).catch(err => console.log(err))
}

//export const CALL_API = Symbol('Call API')