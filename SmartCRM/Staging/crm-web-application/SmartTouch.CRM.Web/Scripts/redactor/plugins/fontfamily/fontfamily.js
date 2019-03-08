// Font Family Plugin
if (!RedactorPlugins) var RedactorPlugins = {};

RedactorPlugins.fontfamily = function () {
    return {
        init: function () {
            var fonts = ['Arial', 'Helvetica', 'Georgia', 'Times New Roman', 'Monospace','Tahoma','Calibri','Verdana','Calibri Light'];
            var that = this;
            var dropdown = {};

            jQuery.each(fonts, function (i, s) {
                dropdown['s' + i] = { title: s, func: function () { that.fontfamily.setFontfamily(s); } };
            });

            dropdown.remove = { title: 'Remove font', func: function () { that.fontfamily.resetFontfamily(); } };

            var button = this.button.add('fontfamily', 'Change Font Family')
            this.button.addDropdown(button, dropdown);
        },
        setFontfamily: function (value) {
            temp = this;
            this.inline.format('font-family', value);
        },
        resetFontfamily: function () {
            this.inline.removeStyleRule('font-family');
        }
    }
};