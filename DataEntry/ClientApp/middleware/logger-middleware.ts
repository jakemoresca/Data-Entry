import { Middleware, MiddlewareAPI, Dispatch, Action } from "redux";
import { ApplicationState } from '../store';

export interface ExtendedMiddleware<StateType> extends Middleware {
    <S extends StateType>(api: MiddlewareAPI<S>): (next: Dispatch<S>) => Dispatch<S>;
}
 
export const loggerMiddleware: ExtendedMiddleware<ApplicationState> = <S extends ApplicationState>(api: MiddlewareAPI<S>) =>
    (next: Dispatch<S>) =>
        <A extends Action>(action: A): A => {
            console.log(`Before ${action.type}`);
            console.log(api.getState());
            const result = next(action);
            console.log(`After ${action.type}`); // Can use: api.getState()
            console.log(api.getState());
            return result;
        };