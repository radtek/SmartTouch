function St_Draggable(classselector) {
    $("." + classselector).draggable({
        revert: "invalid",
        helper: "clone",
        cursor: "move",
        opacity: "0.5",
        connectToSortable: true,
        appendTo: "body",
        start: function (event, ui) {
            $(ui.helper).addClass('stc-dragging-control');
            $(this).closest("." + classselector).addClass('drag-widget');
        },
        drag: function (event, ui) {
            var appendingElement = '<div class="stc-dropping-here"></div>';
            //console.log('event.clientY: ' + event.clientY);
            GetNearestElement(ui, appendingElement, '#campaignsdesignarea > .st-drop-valid.st-droparea > div');
        },
        
        stop: function (event, ui) {
            //$(this).find('a').animate({ opacity: 1 }, 1000);
            $(this).closest("." + classselector).removeClass('drag-widget');
            $('.stc-dropping-here').remove();
        }
    })

}
function GetNearestElement(draggableHelper, appendingElement, nearestSelector) {
    var nearestCoords = '{ "x":' + draggableHelper.helper.offset().left + ', "y":' + draggableHelper.helper.offset().top + '}';
    nearestCoords = $.parseJSON(nearestCoords);
    var nearestLayout = $.nearest(nearestCoords, nearestSelector);
    if (nearestLayout.length > 0) {
        var wooPrepOrApp = ((draggableHelper.offset.top - nearestLayout.offset().top)) < 0 ? 1 : 0; // 1 = Append, 0 = Prepend
        $('.stc-dropping-here').remove();
        if (wooPrepOrApp) {
            $.nearest(nearestCoords, nearestSelector).before(appendingElement);
        } else {
            $.nearest(nearestCoords, nearestSelector).after(appendingElement);
        }
    }
    else {
    }
}

function St_DropableByclone(classselector, AcceptSelector) { }

function St_AnimatedAppend(Appender, Appendto) {
    $(Appender).prependTo(Appendto).effect("fade", {}, 1000, function () {
        $(Appender).removeAttr("style").hide().fadeIn();
    });
}

function St_Sortable(classselector) {
   // console.log(classselector);
    $("." + classselector).sortable({ handle: ".sort" });
}
$(function () {
    Reset();
});

function Reset() {

    St_Draggable('item');
    St_Draggable('dropimg');
    St_Draggable('formitem');
    St_Sortable('campaigns-list');

}

setTimeout(function () {
    St_Draggable('formitem');
}, 00);

function Radio() {
    $('[data-toggle="radio"]').each(function () {
        var $radio = $(this);
        $radio.radio();
    });
}

function Checkbox() {
    $('[data-toggle="checkbox"]').each(function () {
        var $checkbox = $(this);
        $checkbox.checkbox();
    });
}
function St_Delete(Element) {
    var app = $(Element).closest(".st-layout").parent('li');
    $(app).delay(200).fadeOut(1000);
    $(app).animate({ "opacity": "0", }, { "complete": function () { $(app).remove(); } });
}

function editor() {
    $(".editable").kendoEditor({
        tools: [
            "bold",
            "italic",
            "underline",
            "strikethrough",
            "justifyLeft",
            "justifyCenter",
            "justifyRight",
            "justifyFull",
            "insertUnorderedList",
            "insertOrderedList",
            "createLink",
            "unlink",
            "insertImage",
            "viewHtml",
            "backColor",
            "fontName",
            "fontSize",
            "foreColor"
        ],
        select: kendoEditorSelect
    });
}
function kendoEditorSelect(e) {
    $(".editable .k-br").remove();
}
function applyKendoEditor() {
    $(".editable").kendoEditor({
        tools: [
            "bold",
            "italic",
            "underline",
            "strikethrough",
            "justifyLeft",
            "justifyCenter",
            "justifyRight",
            "justifyFull",
            "insertUnorderedList",
            "insertOrderedList",
            "createLink",
            "unlink",
            "insertImage",
            "viewHtml",
            "backColor",
            "fontName",
            "fontSize",
            "foreColor"
        ],
        select: kendoEditorSelect
    });
}

function imageEditor() {
    $(".imageeditor").kendoEditor({
        tools: [
            "justifyLeft",
            "justifyCenter",
            "justifyRight",
            "createLink",
            "unlink",
            "viewHtml"
        ],
        select: kendoEditorSelect
    });
}

$(document).ready(function () {
    editor();
    applyKendoEditor();
    imageEditor();
});

