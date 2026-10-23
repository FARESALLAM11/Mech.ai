const fs = require('fs');
const root = 'D:/Mech/src-tauri/src/';
let computer = fs.readFileSync(root + 'tools/computer.rs', 'utf8');
computer = computer.replace(`    process
        .args([
            "-NoLogo",
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-Command",
            &command,
        ])`, `    process
        .args([
            "-NoLogo",
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
        ])
        .arg("-Command")
        .arg(&command)`);
computer = computer.split('â€¦').join('…');
fs.writeFileSync(root + 'tools/computer.rs', computer, 'utf8');

let cli = fs.readFileSync(root + 'tools/cli.rs', 'utf8');
cli = cli.replace(`    command.args([
        "/d",
        "/s",
        "/c",
        &format!("\\"{}\\" --version", path.display()),
    ]);`, `    command
        .args(["/d", "/s", "/c"])
        .arg(format!("\\"{}\\" --version", path.display()));`);
cli = cli.replace(`    command
        .args([
            "-NoLogo",
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-Command",
            &script,
        ])`, `    command
        .args([
            "-NoLogo",
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
        ])
        .arg("-Command")
        .arg(&script)`);
cli = cli.replace('command.args(["/PID", &pid.to_string(), "/T", "/F"]);', 'command.arg("/PID").arg(pid.to_string()).args(["/T", "/F"]);');
cli = cli.split('â€¦').join('…');
fs.writeFileSync(root + 'tools/cli.rs', cli, 'utf8');

// Fix any mojibake in Rust and TS sources.
function walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = dir + '/' + entry.name;
    if (entry.isDirectory()) walk(p);
    else if (/\.(rs|ts|tsx)$/.test(entry.name)) {
      const original = fs.readFileSync(p, 'utf8');
      const fixed = original.split('â€¦').join('…');
      if (fixed !== original) { fs.writeFileSync(p, fixed, 'utf8'); console.log('fixed', p); }
    }
  }
}
walk('D:/Mech/src-tauri/src');
walk('D:/Mech/src');
console.log('done');
