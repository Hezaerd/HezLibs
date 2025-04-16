const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

try {
  console.log('Running setup...');

  // Install Git hooks
  console.log('Installing Git hooks...');
  execSync('npm run install-hooks', { stdio: 'inherit' });

  console.log('Setup completed successfully!');
} catch (error) {
  console.error('Setup failed:', error);
  process.exit(1);
}