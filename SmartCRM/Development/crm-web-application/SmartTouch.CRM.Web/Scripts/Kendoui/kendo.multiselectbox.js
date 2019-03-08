(function ($) {

    // shorten references to variables. this is better for uglification
    var kendo = window.kendo,
        ui = kendo.ui,
        DropDownList = ui.DropDownList,
        SELECT = "select",
        SELECTED = "k-state-selected";

    var MultiSelectBox = DropDownList.extend({

        init: function (element, options) {
            options.template = kendo.template(
                kendo.format('<span data-value="#= {0} #">#= {1} #</span>',
                    options.dataValueField,
                    options.dataTextField
                )
            );
            // base call to widget initialization
            DropDownList.fn.init.call(this, element, options);
        },

        current: function (candidate) {
            return this._current;
        },

        options: {
            name: "MultiSelectBox",
            index: -1
        },

        _focus: function (li) {
            var that = this
            if (that.popup.visible() && li && that.trigger(SELECT, { item: li })) {
                that.close();
                return;
            }
            that._select(li);
        },

        _select: function (li) {
            var that = this,
                 current = that._current,
                 data = that._data(),
                 value,
                 text,
                 idx, hasOptionLabel;

            li = that._get(li);

            if (that.options.optionLabel) hasOptionLabel = true;
            else hasOptionLabel = false;
            
            if (li && li[0]) {
                idx = ui.List.inArray(li[0], that.ul[0]);
                if (idx > -1) {
                    if (li.hasClass(SELECTED))
                        li.removeClass(SELECTED);
                    else {
                        li.addClass(SELECTED);
                        that.current(li);
                    }
                    var selecteditems = $(that.ul[0]).children("li." + SELECTED);
                    value = [];
                    text = [];
                    $.each(selecteditems, function (indx, item) {
                        var obj = $(item).children("span").first();
                        value.push(obj.attr("data-value"));
                        text.push(obj.text());
                    });

                    that._textAccessor(text.join(", "));
                    that._accessor(value !== undefined ? value : text, idx);
                }
            }

        },

        value: function (value) {
            
            var that = this,
                idx,
                valuesList = [];
            
            console.log("Value : " + value);
            if (value !== undefined) {
                
                if (!$.isArray(value)) {
                    valuesList.push(value);
                }
                else {
                    valuesList = value;
                }

                $(that.ul[0]).children("li." + SELECTED).removeClass(SELECTED);
                
                $.each(valuesList, function (indx, item) {
                    if (item !== null) {
                        item = item.toString();
                    }

                    that._valueCalled = true;
                    
                    var dataSource = that.dataSource;
                    if (dataSource != null) {
                        var data = dataSource.data();
                        if (data != null && data.length > 0) {
                            $.each(data, function (index, innerItem) {
                                if (innerItem.FieldId.toString() == item) {
                                    var selectedli = $(that.ul[0]).children("li");
                                    if (selectedli != null) {
                                        particularli = selectedli[index];
                                        particularli.className = particularli.className + " k-state-selected";
                                    }
                                }
                            })
                        }
                    }
                });
                return value;
            }
            else {
                //var values = that._accessor().split(',')
                //if (values[0] == "")
                //    return [];
                //else
                //    return values;
            }
        }

    });

    ui.plugin(MultiSelectBox);

})(jQuery);