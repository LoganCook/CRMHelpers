function getIdFromEamil(email) {
    return "id" + email.replace("@", "_").replace(/\./g, "_").replace("+", "_");
}
function checkCRM(email, id) {
    $.ajax("/api/contact/" + email).done(function (data) {
        var content = "No";
        if (data && "contactid" in data) {
            content = "Yes <a href='Contact/Edit/" + data["contactid"] + "'>Edit</a>";
        }
        $('#' + id).html(content);
    });
}
//# sourceMappingURL=indexpage.js.map