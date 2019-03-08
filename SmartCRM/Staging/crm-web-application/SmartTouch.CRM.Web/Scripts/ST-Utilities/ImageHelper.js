if (!imageHelper)
    var imageHelper = {};

imageHelper.dimensions = function (e) {
    return {
        set: function (e) {
            var imageWidth = $(e).width();
            var imageHeight = $(e).height();
            $(e).attr({ 'width': imageWidth, 'height': 'auto' });
            $(e).css({ 'width': imageWidth, 'height': 'auto' });
        },
        get: function (e) {
            return {
                'width': $(e).width(),
                'height': $(e).height()
            }
        }
    }
}