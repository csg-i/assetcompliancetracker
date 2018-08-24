$.onDataEvent('totalTable',
    '#executivescorecard',
    function (v) {
        $(this)[v === true ? 'addClass' : 'removeClass']('total');
    });
$.onDataEvent('setSupervisor',
    'li.supervisor',
    function (data) {
        var $t = $(this)[data.supervisorId ? 'fadeIn' : 'fadeOut']();
        if (data.supervisorId) {
            $t.find('a').data('url', data.supervisorUrl).html(data.supervisorName).data('hash', data.supervisorId).data('name', data.supervisorName);
        }
    });
$.onDataEvent('setEmployee',
    'li.employee',
    function (data) {
        var $t = $(this)[data.employeeId ? 'fadeIn' : 'fadeOut']();
        if (data.employeeId) {
            $t.find('a').html(data.employeeName).data('hash', data.employeeId).data(name, data.employeeName);
        }
    });
$.onDataEvent('showEmployeeSearch',
    '#employeesearch',
    function(b) {
        $(this)[b === true ? 'slideDown' : 'slideUp']('fast', function () {
            $(this).find(':input').selectRange();
        });
    });
$.onAction('reloadScoreCard',
    function(e, data) {
        $(data.target).trigger('reloadscorecard', [data.url, data.hash, data.name]);
    });
$.onAction('employeeSearch',
    function (e, data) {
        $(this).data('hidden', !data.hidden);
        $.triggerDataEvent('showEmployeeSearch', data.hidden === true);
    });

$.onAction('employeeScoreCardPopUp',
    function (e, data) {
        jay.modal({
            title: 'Owner Scorecard',
            size: 'xl',
            partialUrl: data.url
        });
    });
$.onAction('totalOrPci',
    function () {
        var val = $(this).closest('div').find(':radio').getInputValues();
        $.triggerDataEvent('totalTable', val.total === 'true');
    });
$(function () {
    $('#employee').on('typeaheadsuccess',
        function() {
            $.triggerDataEvent('showEmployeeSearch', false);
        });

    $('#executivescorecard')
        .on('scorecardempty',
            function (e, data) {
                $(this).message('info', 'This employee does not have any direct reports.');
                $.triggerDataEvent({
                    setSupervisor: data,
                    setEmployee: data
                });

            })
        .on('scorecardloaded',
            function (e, data) {
                $.triggerDataEvent({
                    setSupervisor: data,
                    setEmployee: data
                });
            });
});