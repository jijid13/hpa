//Requirements
var requestify = require('requestify');
var fs = require("fs");
var YQL = require('yqlp');

//Global Variables
var ret = "";

//Json properties
var contents = fs.readFileSync(require('path').resolve(__dirname, 'properties.json'));
var jsonContent = JSON.parse(contents);

//Action when the get is called
actionGet = function (req, res) {
    const res_data = JSON.parse(JSON.stringify(req.body));
    if (res_data.hasOwnProperty("question")) {
        getWeather(res_data, function () {
            res.send(ret);
        });
    } else {
        res.send(JSON.stringify({ status: "error", answer: "commande inconnue !" }));
    }
}

//Export the constructor
module.exports = function (app) {
    app.post(jsonContent.Module_url, actionGet);
}

//Get Weather
var getWeather = function (semantics, callback) {
    var util = require('util');
    YQL.execp("select * from weather.forecast where woeid in (select woeid from geo.places(1) where text='" + semantics.location + "')  and u='c'")
        .then(function (response) {
            var results = response.query.results;
            var output = util.format('Le temps à %s %s est %s, la température est de %s degrés %s',
                results.channel.location.city,
                results.channel.location.country,
                jsonContent.conditions[results.channel.item.condition.code],
                results.channel.item.condition.temp,
                results.channel.units.temperature
            );

            ret = JSON.stringify({ status: "ok", answer: output });
            callback();// <=== Call callback
        }, function (error) {
            ret = JSON.stringify({ status: "error", answer: response.getCode() });
            callback();// <=== Call callback
        });
}


