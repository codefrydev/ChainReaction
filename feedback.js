window.blazorFunctions = {
    BhukampLao: function (shouldVibrate, shouldPlay) {
        if (shouldVibrate) {
            window.navigator.vibrate(20); 
        }
        if (shouldPlay) {
            var audio = new Audio('bomb.mp3');
            audio.play();
        }
    }
}; 