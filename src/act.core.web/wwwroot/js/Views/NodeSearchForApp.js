$.onDataEvent('updateAssignedData',
    '#results',
    function (data) {
        var $t = $(this);
        var myData = $t.data('ids') || [];
        if ($.isArray(data)) {
            myData = myData.concat(data);
        } else {
            myData.push(data);
        }
        $t.data('ids', myData);
    });
$.onDataEvent('updateClassOnResults',
    '#assigned, #results',
    function (c) {
        $(this).removeClass(function (index, className) {
            return (className.match(/(^|\s)filter-\S+/g) || []).join(' ');
        }).addClass('filter-' + c);
    });
$.onDataEvent('removeAssignedData',
    '#results',
    function (data) {
        var $t = $(this);
        var myData = $t.data('ids') || [];
        $t.data('ids', _.without(myData, data));
    });
$.onDataEvent('findAndUnassign',
    '#assigned',
    function (id) {
        $('div.list-group-item[data-id="' + id + '"]', this).doAction('unassign');
    });
$.onDataEvent('findAndRemoveActive',
    '#results',
    function (id) {
        $('div.list-group-item[data-id="' + id + '"]', this).removeClass('active');
    });
$.onDataEvent('addToAssigned',
    '#assigned',
    function (clone) {
        $(this).prepend(clone);
    });
$.onDataEvent('message',
    '#messageArea',
    function (args) {
        $.fn.message.apply(this, args);
    });
$.onAction('unassign',
    function (e, data) {
        var $t = $(this);
        jay.ajax({
            url: data.assignUrl
        }).done(function () {
            $t.slideUp('fast',
                function () {
                    $(this).remove();
                });
            $.triggerDataEvent({
                removeAssignedData: data.id,
                findAndRemoveActive: data.id
            });
        }).error(function (err) {
            $.triggerDataEvent('message', ['error', err]);
        }).fail(function () {
            $.triggerDataEvent('message', ['error', jay.failMessage]);
        });
    });

$.onAction('assign',
    function (e, data) {
        var $t = $(this);
        if ($t.hasClass('active')) {
            $.triggerDataEvent('findAndUnassign', data.id);
            $t.removeClass('active');
        } else {
            jay.ajax({
                url: data.assignUrl,
                data: {
                    specId: data.specId
                }
            }).done(function () {
                $.triggerDataEvent({
                    addToAssigned: $t.clone().wireUpAssigned(),
                    updateAssignedData: data.id
                });
                $t.addClass('active');
            }).error(function (err) {
                $.triggerDataEvent('message', ['error', err]);
            }).fail(function () {
                $.triggerDataEvent('message', ['error', jay.failMessage]);
            });
        }
    });
$.onAction('wireUpResultsDoneHandler',
    function(e, data) {
        $(this).on('loadcomplete',
            function () {
                var $t = $(this);
                var ids = $t.data('ids');
                $('div.list-group-item', $t).wireUpSearchResult(data.specId);
                _.each(ids,
                    function (id) {
                        $('div.list-group-item[data-id="' + id + '"]', $t).addClass('active');
                    });
            });
    });
$.onAction('wireUpAssigned',
    function (e, data) {
        $('div.list-group-item', this).wireUpAssigned();
        $.triggerDataEvent('updateAssignedData', data.assigned);
    });

$.fn.wireUpAssigned = function () {
    return this.each(function () {
        $(this).removeClass('action-assign').addClass('action-unassign');
    });
};
$.fn.wireUpSearchResult = function (specId) {
    return this.each(function () {
        $(this).addClass('action-assign').data('specId', specId);
    });
};
