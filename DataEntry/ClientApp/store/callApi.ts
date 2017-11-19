import * as jQuery from 'jquery';

export function callApi(endpoint: any, authenticated: any, config: Object = {})
{
    let token = localStorage.getItem('access_token') || null
    //let config = {}

    if (authenticated)
    {
        if (token)
        {
            config = {
                ...config,
                headers: { 'Authorization': `Bearer ${token}` }
            };
        }
        else
        {
            throw "No token saved!";
        }
    }

    return jQuery.ajax(endpoint, config); //fetch(endpoint, config);
}