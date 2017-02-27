//Requirements
var requestify = require('requestify');
var fs = require("fs");

//Global Variables
var ret = "";

//Json properties
var contents = fs.readFileSync(require('path').resolve(__dirname, 'properties.json'));
var jsonContent = JSON.parse(contents);

//Action when the get is called
actionGet = function(req, res) {
    const res_data = JSON.parse(JSON.stringify(req.body));
    if (res_data.hasOwnProperty("lumiere")) {
        pleayScenar(res_data, function () {
            res.send(ret);
        });
    } else if (res_data.hasOwnProperty("Temperature")) {
        getTemperature(res_data, function () {
            res.send(ret);
        });
    }else if (res_data.hasOwnProperty("Alarme")) {
        setAlarme(res_data, function () {
            res.send(ret);
        });
    }else {
        res.send("commande inconnue !");
    }
}

//Export the constructor
module.exports = function(app) {
    app.post(jsonContent.Module_url, actionGet);
}

//Play Scenario
var pleayScenar = function(semantics, callback) {
    requestify.post(jsonContent.mf_urlToken, {
        grant_type: 'password',
        client_id: jsonContent.client_id,
        client_secret: jsonContent.client_secret,
        username: jsonContent.username,
        password: jsonContent.password
    })
        .then(function (response) {
            var token = response.getBody();
            var lines = semantics.lumiere.split('|');
            for (var pos in lines) {
                requestify.post(jsonContent.mf_ScenarioUrl
                    .replace('%site', jsonContent.site_id)
                    .replace('%scenario', lines[pos])
                    .replace('%token', token.access_token), {
                })
                .then(function (response) {
                    ret = JSON.stringify({ status: "ok", answer: "c'est fait" });
                    callback();// <=== Call callback
                })
                .fail(function(response) {
                    ret = JSON.stringify({ status: "error", answer: response.getCode()});
                    callback();// <=== Call callback
                });
            }

        })
        .fail(function(response) {
            ret = JSON.stringify({ status: "error", answer: response.getCode()});
            callback();// <=== Call callback
        });
}

//Get Temperature
var getTemperature = function (semantics, callback) {
    requestify.post(jsonContent.mf_urlToken, {
        grant_type: 'password',
        client_id: jsonContent.client_id,
        client_secret: jsonContent.client_secret,
        username: jsonContent.username,
        password: jsonContent.password
    })
        .then(function (response) {
            var token = response.getBody();
            requestify.get(jsonContent.mf_TemperatureUrl
                .replace('%site', jsonContent.site_id)
                .replace('%token', token.access_token), {
                })
            .then(function (response) {
                var temperatures = response.getBody();
                var rooms = jsonContent.temperature_rooms.split('|');
                ret = "La température de "
                for (var pos in rooms) {
                    ret += rooms[pos].split(',')[0] + " est " + temperatures.payload.items[rooms[pos].split(',')[1]].lastTemperature + " degrés et de ";
                }
                ret = ret.substring(0, ret.length - 6);
                callback();// <=== Call callback
            })
            .fail(function (response) {
                ret = JSON.stringify({ status: "error", answer: response.getCode() });
                callback();// <=== Call callback
            });
        })
        .fail(function (response) {
            ret = JSON.stringify({ status: "error", answer: response.getCode() });
            callback();// <=== Call callback
        });
}

//Set alarme
var setAlarme = function (semantics, callback) {
    requestify.post(jsonContent.mf_urlToken, {
        grant_type: 'password',
        client_id: jsonContent.client_id,
        client_secret: jsonContent.client_secret,
        username: jsonContent.username,
        password: jsonContent.password
    })
        .then(function (response) {
            var token = response.getBody();
            requestify.get(jsonContent.mf_setSecurityUrl
                .replace('%site', jsonContent.site_id)
                .replace('%level', semantics.Alarme)
                .replace('%token', token.access_token), {
                })
            .then(function (response) {
                ret = JSON.stringify({ status: "ok", answer: "c'est fait" });
                callback();// <=== Call callback
            })
            .fail(function (response) {
                ret = JSON.stringify({ status: "error", answer: response.getCode() });
                callback();// <=== Call callback
            });
        })
        .fail(function (response) {
            ret = JSON.stringify({ status: "error", answer: response.getCode() });
            callback();// <=== Call callback
        });
}