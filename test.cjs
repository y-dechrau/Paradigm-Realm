const assert=require('node:assert/strict'),R=require('./engine.js');
const s=R.create(42);assert.deepEqual(s,R.create(42));assert.equal(s.maps[0].length,1440);
let settler=s.units.find(u=>!u.owner&&u.type==='Settler');const city=R.found(s,settler);assert(city);assert(!s.units.includes(settler));
const worker=s.units.find(u=>!u.owner&&u.type==='Worker');assert(R.improve(s,worker,'road'));assert.equal(worker.moves,0);assert(!R.improve(s,worker,'farm'));R.endTurn(s);s.tech.push('Agriculture');assert(R.improve(s,worker,'farm'));
const warrior=s.units.find(u=>!u.owner&&u.type==='Warrior');R.tile(s,warrior.x+1,warrior.y).type='grass';assert(R.move(s,warrior,warrior.x+1,warrior.y));assert.equal(warrior.moves,2);assert(!R.move(s,warrior,warrior.x+3,warrior.y));R.tile(s,warrior.x+1,warrior.y).type='ocean';assert(!R.move(s,warrior,warrior.x+1,warrior.y));
const enemy=R.addUnit(s,1,'Warrior',warrior.x,warrior.y+1,0);R.tile(s,enemy.x,enemy.y).type='grass';assert(!R.move(s,warrior,enemy.x,enemy.y));s.relations[1]='war';assert(R.move(s,warrior,enemy.x,enemy.y));assert(enemy.hp<10);
s.relations[1]='peace';for(let i=0;i<130;i++)R.endTurn(s);assert(s.tech.includes('Gate Theory'));assert(s.units.some(u=>!u.owner&&u.id>city.id));assert(R.validSave(JSON.parse(JSON.stringify(s))));assert(!R.validSave({version:1}));
city.buildings.push('Gate');const colonist=R.addUnit(s,0,'Settler',city.x,city.y,0);assert(R.gate(s,colonist));assert.equal(colonist.z,1);colonist.moves=4;assert(R.found(s,colonist));assert.match(s.result,/Realm victory/);
console.log('PASS: deterministic world, founding, improvements, movement, diplomacy/combat, 130-turn simulation, technology, save validation, gate and victory.');
