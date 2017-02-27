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
actionGet = function(req, res) {
    const res_data = JSON.parse(JSON.stringify(req.body));
    if (res_data.hasOwnProperty("question")) {   
        getWeather(res_data, function () {
            res.send(ret);
        });
    }else{
        res.send("commande inconnue !");
    }
}

//Export the constructor
module.exports = function(app) {
    app.post(jsonContent.Module_url, actionGet);
}

//Get Weather
var getWeather = function(semantics, callback) {
    var util = require('util');
    YQL.execp("select * from weather.forecast where woeid in (select woeid from geo.places(1) where text=@location)  and u='c'", { location:semantics.Location })
        .then(function(response) {
            var results = response.query.results;
            var output = util.format('Le temps à %s %s est %s, la température est de %s degrés %s',
                results.channel.location.city,
                results.channel.location.country,
                getFrenchCondition(results.channel.item.condition.code),
                results.channel.item.condition.temp,
                results.channel.units.temperature
            );

            ret = JSON.stringify({ status: "ok", answer: output });
            callback();// <=== Call callback
        }, function(error) {
            ret = JSON.stringify({ status: "error", answer: response.getCode()});
            callback();// <=== Call callback
        });
}

var getFrenchCondition = function(code) {
    var condition = "";
    switch (code)
    {
        case "0":
            condition = "une tornade est prévue";
            break;
        case "1":
            condition = "une tempête tropicale est prévue";
            break;
        case "2":
            condition = "un ouragan est prévu";
            break;
        case "3":
            condition = "des orages violents sont prévus";
            break;
        case "4":
            condition = "des orages sont prévus";
            break;
        case "5":
            condition = "la pluie et la neige sont prévus";
            break;
        case "6":
            condition = "la pluie et la neige fondue sont prévus";
            break;
        case "7":
            condition = "neige mêlées et le grésil sont prévus";
            break;
        case "8":
            condition = "bruine verglaçante est prévue";
            break;
        case "9":
            condition = "bruine est prévue";
            break;
        case "10":
            condition = "pluie verglaçante est prévue";
            break;
        case "11":
            condition = "des averses sont prévues";
            break;
        case "12":
            condition = "des averses sont prévues";
            break;
        case "13":
            condition = "des averses de neige sont prévues";
            break;
        case "14":
            condition = "des averses de neige sont prévues";
            break;
        case "15":
            condition = "poudrerie prévue";
            break;
        case "16":
            condition = "neige prévue";
            break;
        case "17":
            condition = "grêle prévue";
            break;
        case "18":
            condition = "grésil prévu";
            break;
        case "19":
            condition = "poussière prévue";
            break;
        case "20":
            condition = "brumeux prévu";
            break;
        case "21":
            condition = "brume prévue";
            break;
        case "22":
            condition = "enfumé prévu";
            break;
        case "23":
            condition = "venteux prévu";
            break;
        case "24":
            condition = "venteux prévu";
            break;
        case "25":
            condition = "froid prévu";
            break;
        case "26":
            condition = "temps nuageux";
            break;
        case "27":
            condition = "Partiellement nuageux";
            break;
        case "28":
            condition = "Partiellement nuageux";
            break;
        case "29":
            condition = "Partiellement nuageux";
            break;
        case "30":
            condition = "Partiellement nuageux";
            break;
        case "31":
            condition = "transparent";
            break;
        case "32":
            condition = "ensoleillé";
            break;
        case "33":
            condition = "juste";
            break;
        case "34":
            condition = "juste";
            break;
        case "35":
            condition = "pluie mêlée de grêle";
            break;
        case "36":
            condition = "chaude";
            break;
        case "37":
            condition = "orages isolés";
            break;
        case "38":
            condition = "orages dispersés";
            break;
        case "39":
            condition = "orages dispersés";
            break;
        case "40":
            condition = "averses dispersées";
            break;
        case "41":
            condition = "fortes chutes de neige";
            break;
        case "42":
            condition = "Chutes de neige";
            break;
        case "43":
            condition = "fortes chutes de neige";
            break;
        case "44":
            condition = "éclaircies";
            break;
        case "45":
            condition = "averses orageuses";
            break;
        case "46":
            condition = "averses de neige";
            break;
        case "47":
            condition = "averses orageuses isolées";
            break;
        case "3200":
            condition = "n'est pas disponible";
            break;
    }
    return condition;
}


