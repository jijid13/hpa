var express = require('express'),
    fs = require("fs"),
    bodyParser = require('body-parser');

var app = express()
app.use(bodyParser.json());

//Json properties
var contents = fs.readFileSync(require('path').resolve(__dirname, 'properties.json'));
var jsonContent = JSON.parse(contents);

app.get('/', function (req, res) {
  res.send('Welcome to the Home Personal Assistant')
})

app.listen(jsonContent.port, function () {
  console.log('Example app listening on port ' + jsonContent.port)
})

var controllers = require('require-all')({
  dirname :  __dirname + '/plugins',
  filter  :  /(.+index)\.js$/
});

for (var key in controllers) {
    console.log(`key is listening on ${key}`);
    var obj = require(`./plugins/${key}/index.js`);
    new obj(app);
 }
