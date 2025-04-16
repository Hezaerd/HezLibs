const fs = require('fs');
const path = require('path');

try {
    const gitHooksDir = path.join(__dirname, '..', '.git', 'hooks');
    const hooksSourceDir = path.join(__dirname, 'hooks');
    
    // Ensure .git/hooks directory exists
    if (!fs.existsSync(gitHooksDir)) {
        fs.mkdirSync(gitHooksDir, { recursive: true });
    }

    // Function to copy files recursively
    function copyFileSync(source, target) {
        let targetFile = target;

        // If target is a directory a new file with the same name will be created
        if (fs.existsSync(target)) {
            if (fs.lstatSync(target).isDirectory()) {
                targetFile = path.join(target, path.basename(source));
            }
        }

        fs.writeFileSync(targetFile, fs.readFileSync(source));
    }

    function copyFolderRecursiveSync(source, target) {
        let files = [];

        // Check if folder needs to be created or integrated
        if (!fs.existsSync(target)) {
            fs.mkdirSync(target, { recursive: true });
        }

        // Copy
        if (fs.lstatSync(source).isDirectory()) {
            files = fs.readdirSync(source);
            files.forEach(function (file) {
                const curSource = path.join(source, file);
                const curTarget = path.join(target, file);
                if (fs.lstatSync(curSource).isDirectory()) {
                    copyFolderRecursiveSync(curSource, curTarget);
                } else {
                    copyFileSync(curSource, curTarget);
                }
            });
        }
    }

    // Copy all files from hooksSourceDir to gitHooksDir
    copyFolderRecursiveSync(hooksSourceDir, gitHooksDir);

    console.log('Git hooks installed successfully!');
} catch (error) {
    console.error('Error installing Git hooks:', error);
    process.exit(1);
}