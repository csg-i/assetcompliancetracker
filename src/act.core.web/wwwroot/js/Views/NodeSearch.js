$.onAction('unassignBuildSpec',
    function (evt, data) {
        var $t = $(this);
        var $assign = $t.parent().find('a.action-change-build-spec');
        var $pill = $t.closest('div.list-group-item');

        jay.modal({
            title: 'Confirm Unassign',
            content: $('<p>Are you sure you want to unassign a the build spec ' + data.name + ' to this node?</p>'),
            size: 'small',
            buttons: [
                {
                    label: 'No, Cancel',
                    close:true
                },
                {
                    label: 'Yes, Unassign',
                    cssClass: 'btn-danger',
                    onClick: function(e, content, mw) {
                        mw.showContentLoading();
                        jay.ajax({
                            url: data.url
                        }).done(function () {
                            $t.data('name', '').fadeOut('fast', function () {
                                $assign.html(data.siblingText);
                            });
                            $pill.find('a.spec-report, a.port-report, a.compliance-report').addClass('disabled').attr('disabled', 'disabled').attr('href', '#');
                            mw.hideContentLoading().modal('hide');
                        }).error(function(msg) {
                            mw.hideContentLoading();
                            content.message('error', msg);
                        }).fail(function() {
                            mw.hideContentLoading();
                            content.message('error', jay.failMessage);
                        });
                    }
                }
            ]
        });
    });
$.onAction('changeBuildSpec',
    function (evt, data) {
        var $a = $(this);
        var $clone = $($('#bstemplate').html());
        var $unassign = $a.parent().find('a.action-unassign-build-spec').hide();
        var unassignHasName = $unassign.data().name.length > 0;
        $a.hide().after($clone);

        var $t = $clone.find(':input:text')
            .on('typeaheadstartingsearch.typeahead', function () {
                $t.removeClass('is-valid')
                    .removeClass('is-invalid');
            })
            .on('typeaheadsearchdone.typeahead', function () {
                $t.removeClass('is-invalid')
                    .addClass('is-valid');

            })
            .on('typeaheadfailure.typeahead',
                function() {
                    $t.removeClass('is-valid')
                        .addClass('is-invalid').closest('div.form-group')
                        .append($('<span/>').addClass('form-text  text-danger').html(jay.failMessage));
                })
            .on('typeaheadnoresults.typeahead',
                function () {
                    $t.removeClass('is-valid')
                        .addClass('is-invalid');
                })
            .on('typeaheadsuccess.typeahead',
                function (e, value, text) {
                    var $pill = $t.closest('div.list-group-item');
                    $t.removeClass('is-valid')
                        .removeClass('is-invalid')
                            .closest('div.form-group')
                                .find('span.form-text')
                                    .remove();
                    jay.ajax({
                        url: data.saveUrl,
                        data: {
                            specId: value
                        }
                    }).done(function(json) {
                        $a.html(text).fadeIn('fast',
                            function () {
                                $unassign.data('name', text).fadeIn('fast');
                            });
                        $t.off('.typeahead').typeahead('destroy');
                        if (value) {
                            $pill.find('a.spec-report').attr('href', json.specUrl).removeAttr('disabled').removeClass('disabled');
                            $pill.find('a.port-report').attr('href', json.portUrl).removeAttr('disabled').removeClass('disabled');
                            $pill.find('a.compliance-report').attr('href', json.complianceUrl).removeAttr('disabled').removeClass('disabled');
                        } else {
                            $pill.find('a.spec-report, a.port-report, a.compliance-report').attr('disabled', 'disabled').attr('href', '#').addClass('disabled');
                        }
                        $clone.remove();
                    }).error(function(m) {
                        $t.addClass('is-invalid').closest('div.form-group')
                            .append($('<span/>').addClass('form-text  text-danger').html(m));
                    }).fail(function() {
                        $t.addClass('is-invalid').closest('div.form-group')
                            .append($('<span/>').addClass('form-text  text-danger').html(jay.failMessage));
                    });
                })
            .typeahead({
                ajax: {
                    url: data.url
                },
                valueField: 'i',
                displayField: 't'
            }).focus();

        $clone.find('button.btn-danger').one('click',
            function() {
                $t.off('.typeahead').typeahead('destroy');
                $a.fadeIn('fast', function() {
                    if (unassignHasName === true) {
                        $unassign.fadeIn('fast');
                    }
                });               
                $clone.remove();
            });
    });

$.onAction('convergeReport',
    function (evt, data) {
        jay.ajax({
            url: data.url,
            data: {
                nodeGuid: data.id
            }
        }).done(function (json) {
            var win = window.open(json.url, '_blank');
            if (win) {
                win.focus();
            } else {
                alert('Please allow popups for this website');
            }
        });
    }
);