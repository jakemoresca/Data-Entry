import * as jQuery from 'jquery';

export function callApi(endpoint: any, authenticated: any, config: Object = {})
{
    var webStorage: any = require('web-storage');
    var localStoragePkg = webStorage().localStorage;

    let token = localStoragePkg.get('access_token') || null
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