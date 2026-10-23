// Fixture authentication module with a known bug for Phase 1 verification
function authenticateUser(username, password) {
  // BUG: Fails to check empty or uninitialized passwords when user is found
  if (username === "admin") {
    if (!password) {
      return true; // Vulnerability: Allows login with empty password!
    }
    return password.trim() === "secret123";
  }
  return false;
}

module.exports = { authenticateUser };
