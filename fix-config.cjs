const fs = require('fs');
const confPath = 'D:/Mech/src-tauri/tauri.conf.json';
const conf = JSON.parse(fs.readFileSync(confPath, 'utf8'));
conf.app.windows[0].title = 'Mech — Agent workspace';
fs.writeFileSync(confPath, JSON.stringify(conf, null, 2) + '\n', 'utf8');

let groq = fs.readFileSync('D:/Mech/src-tauri/src/providers/groq.rs', 'utf8');
groq = groq.replace(/            ModelInfo \{\n                id: "mixtral-8x7b-32768"\.into\(\),\n                name: "Mixtral 8x7B 32k"\.into\(\),\n                provider: "groq"\.into\(\),\n                context_window: 32768,\n                input_price_per_m: 0\.24,\n                output_price_per_m: 0\.24,\n                supports_streaming: true,\n            \},\n/, '');
fs.writeFileSync('D:/Mech/src-tauri/src/providers/groq.rs', groq, 'utf8');
console.log('title', conf.app.windows[0].title);
console.log('mixtral remains:', groq.includes('mixtral'));
