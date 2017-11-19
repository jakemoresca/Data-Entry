import * as React from 'react';
import { Link, RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { ApplicationState } from '../store';
import * as AccountState from '../store/Account';

// At runtime, Redux will merge together...
type HomeProps =
    AccountState.AccountState        // ... state we've requested from the Redux store
    & typeof AccountState.actionCreators      // ... plus action creators we've requested
    & RouteComponentProps<{}>;

class Home extends React.Component<HomeProps, {}> {

    handleChange = (event: any) =>
    {
        const e = event.target as HTMLInputElement;
        this.props.updateCurrentLogin(e.name, e.value);
    }

    login = (event: any) =>
    {
        this.props.login(this.props.currentAccount.username, this.props.currentAccount.password);
    }

    public render() {
        return <div>
            <div className="loginmodal-container">
                <h1>Login to Your Account</h1>
                <br />
			  <form>
				<input type="text" name="username" placeholder="Username" onChange={this.handleChange} />
                <input type="password" name="password" placeholder="Password" onChange={this.handleChange} />
                <input type="button" name="login" className="login loginmodal-submit" value="Login" onClick={this.login} />
			  </form>
			</div>
        </div>;
    }
}

export default connect(
    (state: ApplicationState) => state.account, // Selects which state properties are merged into the component's props
    AccountState.actionCreators                 // Selects which action creators are merged into the component's props
)(Home) as typeof Home;
