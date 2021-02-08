const { spawn } = require('child_process')

function resetDataContainer(containerId, done){
    console.log(`resetting data-container ${containerId}...`)

    spawnctl = spawn('spawnctl', ['reset', 'data-container', containerId, '-q'])

    spawnctl.stdout.on('data', function(data){
        if(process.env.SPAWNCTL_DEBUGLOG){
            console.log('spawnctl stdout: ' + data)
        }
    })
    
    spawnctl.on('exit', function (code) {
        if (code !== 0) {
            console.log(`reset data-container exited with code ${code}`)
        }
        done()
    })

    spawnctl.stderr.on('data', (data) => {
        console.error(`spawnctl error:\n${data}`);
    });
}

module.exports = {
    resetDataContainer,
}