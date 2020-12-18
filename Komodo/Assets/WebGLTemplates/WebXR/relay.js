const RELAY_BASE_URL = "https://relay.komodo-dev.library.illinois.edu"
// const RELAY_BASE_URL = "http://localhost:3000"
const API_BASE_URL = "https://api.komodo-dev.library.illinois.edu/";
// const API_BASE_URL = "http://localhost:4040/api/portal/";


// connect to socket.io relay server
var socket = io(RELAY_BASE_URL);

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
var url = API_BASE_URL + "labs/" + session_id.toString();
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

// join session by id
var joinIds = [session_id, client_id]
socket.emit("join", joinIds);

// const startPlayback = function() {
//     console.log('playback started:', playback_id);
//     let playbackArgs = [client_id, session_id, playback_id]
//     socket.emit('playback', playbackArgs);
// }

socket.on('playbackEnd', function() {
    console.log('playback ended');
});

// To prevent the EMFILE error, clear the sendbuffer when reconnecting
socket.on('reconnecting',function(){
    socket.sendBuffer = [];
});

// text chat relay
var chat = io(RELAY_BASE_URL + '/chat');
chat.emit("join", joinIds);
