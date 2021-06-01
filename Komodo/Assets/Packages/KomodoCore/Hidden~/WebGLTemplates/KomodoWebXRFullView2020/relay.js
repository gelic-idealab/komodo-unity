// Replace these with your own server's URLs.

// Tip: if you need to change this on-the-fly, you can edit this file without rebuilding. It's also possible to use the inspector to inspect the VR frame and call `window.RELAY_API_BASE_URL="<your-server-url>"`, if for some reason you need to do that in real time.

var RELAY_BASE_URL = "http://localhost:3000";
var API_BASE_URL = "http://localhost:4040";


// init globals which Unity will assign when setup is done.
var socket = null;
var chat = null;

/**
 * Get the URL parameters
 * source: https://css-tricks.com/snippets/javascript/get-url-variables/
 * @param  {String} url The URL
 * @return {Object}     The URL parameters
 */
var getParams = function (url) {
    var params = {};
    var parser = document.createElement('a');
    parser.href = url;
    var query = parser.search.substring(1);
    var vars = query.split('&');
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split('=');
        params[pair[0]] = decodeURIComponent(pair[1]);
    }
    return params;
};

let params = getParams(window.location.href);
console.log(params);

// Client and session params supplied by portal iframe src
var session_id = Number(params.session);
var client_id = Number(params.client);
var isTeacher = Number(params.teacher) || 0;
var playback_id = Number(params.playback);

// wrapper `details` object to be populated by api call and passed to Unity
// NOTE(rob): Unity cannot deserialize raw arrays
var details = {
    assets: [],
    build: "",
    course_id: 0,
    create_at: "",
    description: "",
    end_time: "",
    session_id: 0,
    session_name: "",
    start_time: "",
    users: []
}

// fetch lab details from portal api
var url = API_BASE_URL + "/labs/" + session_id.toString();
var request = new XMLHttpRequest();
request.open("GET", url, true);
request.responseType = "json";
request.send();

request.onload = function(){
    let res = request.response;
    console.log(res);
    // session details
    details.build = res.build;
    details.course_id = res.course_id;
    details.create_at = res.create_at;
    details.description = res.description;
    details.end_time = res.end_time;
    details.session_id = res.session_id;
    details.session_name = res.session_name;
    details.start_time = res.start_time;

    details.users = res.users;
    
    let assets_response = res.assetList;
    for (idx = 0; idx < assets_response.length; idx++)
    {
        asset = new Object;
        asset.id = assets_response[idx].asset_id;
        asset.name = assets_response[idx].asset_name;
        asset.url = assets_response[idx].path;
        asset.isWholeObject = Boolean(assets_response[idx].is_whole_object);
        asset.scale = assets_response[idx].scale || 1;
        details.assets.push(asset);
    }
};
