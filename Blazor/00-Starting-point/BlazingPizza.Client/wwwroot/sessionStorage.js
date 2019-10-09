(function () {
    window.blazorSessionStorage = {
        get: key => key in sessionStorage ? JSON.parse(sessionStorage[key]) : null,
        set: (key, value) => { sessionStorage[key] = JSON.stringify(value); },
        delete: key => { delete sessionStorage[key]; }
    };
})();
