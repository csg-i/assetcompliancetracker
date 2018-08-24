$(function() {
    $('#ownerscorecard')
        .on('scorecardempty',
            function() {
                $(this).message('info', 'This employee does not have any OS or application specs.');
            })
        .on('scorecardloaded',
            function(e, data) {
                log(data);
            });
});
