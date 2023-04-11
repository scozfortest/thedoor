const admin = require('firebase-admin');
//Json


//Tools
const Probability = require('./Scoz/Probability.js');

console.log("haha");
let weights = [1, 1, 1, 1, 1];
let indices = Probability.GetRandomDistinctIndices(weights, 2);
console.log(indices);
