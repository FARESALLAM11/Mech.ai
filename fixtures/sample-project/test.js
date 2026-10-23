const { authenticateUser } = require('./src/auth');

console.log("Running Mech Test Suite for Sample Project...");

// Test Case 1: Valid credentials
if (!authenticateUser("admin", "secret123")) {
  console.error("FAIL: Test Case 1 failed: Valid user could not authenticate");
  process.exit(1);
}

// Test Case 2: Empty/whitespace password must return false
if (authenticateUser("admin", "   ")) {
  console.error("FAIL: Test Case 2 failed: Whitespace password was accepted (Security Bug!)");
  process.exit(1);
}

// Test Case 3: Empty string password must return false
if (authenticateUser("admin", "")) {
  console.error("FAIL: Test Case 3 failed: Empty password was accepted");
  process.exit(1);
}

console.log("PASS: All authentication tests passed successfully!");
process.exit(0);
