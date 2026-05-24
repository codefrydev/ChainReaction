window.getInnerDimensions = () => {
    return {
        Width: window.innerWidth,
        Height: window.innerHeight
    };
};

window.chainReactionLayout = {
    _handler: null,
    _timeout: null,
    register(dotNetHelper) {
        this.unregister();
        this._handler = () => {
            clearTimeout(this._timeout);
            this._timeout = setTimeout(() => {
                dotNetHelper.invokeMethodAsync('OnWindowResize');
            }, 150);
        };
        window.addEventListener('resize', this._handler);
        window.addEventListener('orientationchange', this._handler);
    },
    unregister() {
        if (this._handler) {
            window.removeEventListener('resize', this._handler);
            window.removeEventListener('orientationchange', this._handler);
            this._handler = null;
        }
        clearTimeout(this._timeout);
    }
};
