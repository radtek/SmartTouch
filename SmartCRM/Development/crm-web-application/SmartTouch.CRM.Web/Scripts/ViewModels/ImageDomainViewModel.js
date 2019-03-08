var imageDomainViewModel = function (data, baseUrl, serviceUrl) {
    var selfImageDomain = this;

    ko.validatedObservable(ko.mapping.fromJS(data, {}, selfImageDomain));

    selfImageDomain.Domain = ko.observable(data.Domain).extend({
        required: {
            message:'[|Image domain is requried|]'
        },
        //pattern: {
        //    message: 'Invalid URL',
        //    params: '(https?:\/\/(?:www\.|(?!www))[^\s\.]+\.[^\s]{2,}|www\.[^\s]+\.[^\s]{2,})'
        //}
    });
    selfImageDomain.Status = ko.observable(data.Status.toString());


    selfImageDomain.save = function (data, event) {
        console.log(123);

        var jsondata = ko.toJSON(selfImageDomain);
        console.log(JSON.stringify(jsondata))
        selfImageDomain.errors.showAllMessages();
        if (selfImageDomain.errors().length > 0)
            return;
        innerLoader('imagedomain');

        var url = baseUrl + '/' + (selfImageDomain.ImageDomainId() == 0 ? 'InsertImageDomain' : 'UpdateImageDomain');
        console.log(url);

        $.ajax({
            url: url,
            type: 'post',
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ 'imageDomainViewModel': jsondata }),

        }).then(function (response) {
            var filter = $.Deferred()
            removeinnerLoader('imagedomain');
            if (response.success) {
                filter.resolve(response)
            } else {
                filter.reject(response.error)
            }
            return filter.promise()
        }).done(function (data) {
            notifySuccess("[|Successfully saved|]");
            setTimeout(function () { window.location.href = location.origin + "/imagedomains" }, setTimeOutTimer);
        }).fail(function (error) {
            notifyError(error);
        })
    };
};