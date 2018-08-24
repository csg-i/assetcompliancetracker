$.onAction('popupResults',
    function (e, data) {
        jay.modal({
            size: 'xl',
            partialUrl: data.url,
            partialData: {
                t: data.searchType,
                q: data.search
            },
            title: 'Results'
        });
    });