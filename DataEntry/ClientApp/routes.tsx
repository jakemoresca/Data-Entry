import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import Home from './components/Home';
import Jobs from './components/Jobs';
import Job from './components/Job';
import Counter from './components/Counter';

export const routes = <Layout>
    <Route exact path='/' component={ Home } />
    <Route path='/counter' component={ Counter } />
    <Route path='/jobs/:startDateIndex?' component={ Jobs } />
    <Route path='/newjob' component={ Job } />
    <Route path='/jobdetail/:jobId?' component={ Job } />
</Layout>;
