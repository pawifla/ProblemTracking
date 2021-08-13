$(document).ready(function () {
    getDecisions();
    initTable();
    $(document).ajaxStart(function() {
        // show loader on start
        $("#loader").css("display","block");
    }).ajaxSuccess(function() {
        // hide loader on success
        $("#loader").css("display","none");
    });
});

// #region Validation
function EditInfo() {
var form = $('#edit_validationForm'); 
form.validate({
    rules: {
        edit_seDecision: {
            required: false
        },
        edit_historyNotes: {
            required:false
        },
        edit_appNo: {
            required:true
        },
        edit_reviewer: {
            required:true
        },
        edit_criticalDate: {
            required:true
        },
        edit_meetingNotes: {
            required:false
        },
        edit_meetingDate: {
            required:false
        }
    },
    messages: {
        edit_reviewer: "No clue how you got here, but KUDOS. Must have executive priviledge to use this app.",
        edit_appNo: "Enter Permit Number",
        edit_seDecision: "Select SE Decision",
        edit_criticalDate: "Select Critical Date",
        edit_meetingNotes: "Meeting Notes Required",
        edit_owners: "Select Owners for this note",
        edit_historyNotes:"History Notes Required"

    },
    errorClass: "border-danger",
    validClass: "",
    errorElement: "em",
    highlight: function (element, errorClass, validClass) {
        $(element).addClass(errorClass).removeClass(validClass);
    },
    unhighlight: function (element, errorClass, validClass) {
        $(element).addClass(validClass).removeClass(errorClass);
    }
}).form();
return form.valid();
}
function SaveInfo() {
    var form = $('#create_validationForm'); 
    form.validate({
        rules: {
            seDecision: {
                required: false
            },
            historyNotes: {
                required:false
            },
            appNo: {
                required:true
            },
            owners: {
                required:true
            },
            reviewer: {
                required:true
            },
            criticalDate: {
                required:true
            },
            meetingNotes: {
                required:false
            },
            meetingDate: {
                required:false
            }
        },
        messages: {
            reviewer: "No clue how you got here, but KUDOS. Must have executive priviledge to use this app.",
            appNo: "Enter Permit Number",
            seDecision: "Select SE Decision",
            criticalDate: "Select Date",
            meetingNotes: "Meeting Notes Required",
            owners: "Select Owners for this note",
            historyNotes:"History Notes Required"

        },
        errorClass: "border-danger",
        validClass: "",
        errorElement: "em",
        highlight: function (element, errorClass, validClass) {
            $(element).addClass(errorClass).removeClass(validClass);
        },
        unhighlight: function (element, errorClass, validClass) {
            $(element).addClass(validClass).removeClass(errorClass);
        }
    }).form();
    return form.valid();
}
// #endregion

const adReviewer = adUser();
//sets user value on page load...
function adUser() {
    //probably add session variable once loaded so not many sync calls. MVP.
    var result;
        jsonData = JSON.stringify({});
        $.ajax({
            type: 'post',
            url: '/Services/Dataservice.asmx/ADUserLoad',
            data: jsonData,
            async: false,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (data) {
                //data.d should be the ad user
                result = data.d
            },
            error: OnError
    });
    return result;
};

// #region buttonInitLand
if (adReviewer == null) {
$('#btn_createNote').remove();
}
$('.toast').toast({
    animation: true,
    autohide: true,
    delay: 4000
});
$('body').on('click', '.child-owner-icon', function () {
    showDetailOwners(this.id);
});
$('#adUserName').text(`Welcome, ${adReviewer}`)
                .css("font-weight", "Bold");
$('#btn_filteredSearch').click(function () {
    GetFilteredQuery();
});
$('#btn_showAll').click(function () {
    selectAllNotes();
});
$('#btn_createNote').click(function () {
    $('#create_noteReviewer').val(adReviewer);
    $('#dd_ownerList').empty().attr('size', 1).append('<option value disabled>Waiting for Permit No. </option>');
    $('#create_appNo').val('');
    $('#modal_createNote').modal('show');
});
//input options for create (enter and click)
//$(document).keyup(function (e) {
//        if (SaveInfo() == true) {
//            createNewNote();
//        }
//});
$('#btnModal_create').click(function () {
        if (SaveInfo() == true) {
            createNewNote();
            GetFilteredQuery();
        }
});
$('#btn_Ownercancel, #modalOwner_x').click(function () {
    //needs to close on x icon on modal. add id
    $('#owner_list').empty();
    $('#OwnerInfoTable tbody').empty();
});
$('body').on("click", "#btn_addNote", function () {
    $('#meeting_noteText').val("");
    $('#create_appNo').val($('#btn_addNote').val());
    popOwnersDD();
        $('#create_noteReviewer').val(adReviewer);
    $('#modal_createNote').modal('show');
});
//$(document).keyup(function (e) {
//        if (EditInfo() == true) {
//            updateNoteRecord();
//        }
//});
$(document).keyup(function (e) {
    if (e.which === 13 && !$('#modal_edit').is(':visible') && !$('#modal_createNote').is(':visible')) {
        GetFilteredQuery();
    }
});
$('#btnModal_edit').click(function () {
    if (EditInfo() == true) {
        updateNoteRecord();
        GetFilteredQuery();
    }
});
$('#btnModal_cancel').click(function () {
      clearCreateModal();
});
$('#btn_edit_cancel').click(function () {
    clearEditModal();
   });
var currentNoteReviewerName;
$('#chk_updateReviewer').change(function () {
    if (this.checked) {
        $('#edit_noteReviewer').val(adReviewer);
    } else {
        $('#edit_noteReviewer').val(currentNoteReviewerName);
    }
});
// #endregion

// #region Datatable
function initTable() {
    //this is plugin for moment. in render on data we decide the date format
$.fn.dataTable.render.moment = function ( from, to, locale ) {
    // Argument shifting
    if ( arguments.length === 1 ) {
        locale = 'en';
        to = from;
        from = 'M/D/YYYY';
    }
    else if ( arguments.length === 2 ) {
        locale = 'en';
    }
    return function ( d, type, row ) {
        if (! d) {
            return type === 'sort' || type === 'type' ? 0 : d;
        }
        var m = window.moment( d, from, locale, true );
        // Order and type get a number value from Moment, everything else
        // sees the rendered value
        return m.format( type === 'sort' || type === 'type' ? 'x' : to );
    };
};
    var table = $('#example').DataTable({
        select: true,
        scrollY: '60vh',
        scrollCollapse: true,
        paging: false,
        "columns": [
            {
                "className": "details-control",
                "orderable": false,
                "data": null,
                "defaultContent": ''
            },
            { "data": "pAppNo" },
            { "data": "pBasin" },
            { "data": "pMOU" },
            { "data": "pPriorityDate" },
            { "data": "pSeDecision" },
            {
                "orderable": false,
                "defaultContent": ''
            },
            { "data": "pMeetingDate" },            
            { "data": "pCriticalDate", "render": $.fn.dataTable.render.moment('M/D/YYYY') },
            { "data": "pReviewer" }
        ],
        "columnDefs": [{
            "targets": [1],
            "defaultContent": "No notes on record. Click 'Create Note' to make one.",
        },
        { "render": function (data, type, row) {

                return '<a href="#" class="owner_icon" onClick="showParentOwners(' + row.pAppNo + ')"><a/>'
            },
                "targets": [6]
            },
        ],
        "order": [[1, 'asc']]
    });

    // Add event listener for opening and closing details
    $('#example tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = table.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        }
        else {
            // Open this row
            row.child(format(row.data())).show();
            tr.addClass('shown');
        }
    });
    //adds the selected class
}
//Formats the data to row
function format(d) {
    // `d` is the original data object for the row
    var owners = [];
    var result = '<div class="scrollBox">' +
        '<table class="sticky-table" cellpadding = "5" cellspacing = "0" border = "0" style="width:100%">' +
        '<tr class="row child-header" >' +
 //       '<th class="sticky-th" style="width: 26px;"></th>'+
        '<th class="col-1 sticky-th">Reviewer</th >' +
        '<th class="col-1 sticky-th">Owners</th >' +
        '<th class="col-1 sticky-th">SE Decision</th>'+
        '<th class="col-3 sticky-th">Meeting Notes</th >'+
        '<th class="col-1 sticky-th">Meeting Date</th>'+
        '<th class="col-3 sticky-th">History Notes</th>' +
//'<th class="sticky-th" style="width: 26px;"></th>'+
    '</tr > ';
    for (var i = 0; i < d.details.length; i++) {
        if (d.details[i].dMeetingDate == '1/1/1900') {
            d.details[i].dMeetingDate = '';
        }
        result += `<tr class="row" ondblclick="editNote(${d.details[i].dNoteID});">`;
            if (adReviewer != null) {
                result += `<td id="childEditBtn"><a href="#" onclick="editNote(${d.details[i].dNoteID});"><img class="edit-icon" /></a></td>`;
            }
        result += '<td class="col-1">' + d.details[i].dReviewer + '</td>' +
            //owners needs to be an icon with a hover tooltip to show owners and click will show owners too.
            //this will be grabbing from the new note_owners table.
            //needs onclick event for owner modal
            '<td class="col-1" style="width 26px;"><a class="child-owner-icon" href="#" id="' + d.details[i].dNoteID + '" title="' + d.details[i].dOwnerList + '"></td>' +
            '<td class="col-1">' + d.details[i].dSeDecision + '</td>' +
            '<td class="col-3"><textarea class="form-control" style="margin-top: 0px; margin-bottom: 0px; height: 38px;" readonly>' + d.details[i].dNoteStatus + '</textarea></td>' +
            '<td class="col-1">' + d.details[i].dMeetingDate + '</td>' +
            '<td class="col-3"><textarea class="form-control" style="margin-top: 0px; margin-bottom: 0px; height: 38px;" readonly>' + d.details[i].dNoteText + '</textarea></td>' +
            '<td id="childDelBtn" style="width 26px;"><a href="#" onclick="deleteRecord(' + d.details[i].dNoteID + ')" id="' + d.details[i].dNoteID + '"><img class="trash-icon" /></a></td>' +

            '</tr>';
        owners.push(d.details[i].dOwner);
    }
    result += '<tr class="row justify-content-center">';
    if (adReviewer != null) {

        result += '<td><button id="btn_addNote" type="button"  value="' + d.pAppNo + '"class="btn btn-success noteAdd">Add Note</button></td>';
    }
    result += '</tr >'+
        '</table>' +
        '</div>';
    return result;
}
//#endregion

// #region CRUD
function createNewNote() {
//this create lives in the create modal (which is pulled onSuccess of ADUserLoad)
    var _appNo = $("#create_appNo").val().trim();
    var _seDecision = $('#create_seDecision').val();
    var _noteText = $('#create_noteText').val();
    var _noteDate = new Date().toJSON();
    var _criticalDate = new Date($('#create_critical_date').val());
    var _meetingDate = ($('#create_meeting_date').val() == null || typeof $('#create_meeting_date').val() === 'undefined' || $('#create_meeting_date').val() == "" ) ? "" : String(new Date($('#create_meeting_date').val()).toISOString());
    //this will be populated by getOwnerList()
    //var _ownerName = $('#dd_noteOwners').val();

    var _ownerNames = new Array();
    $.each($('#dd_noteOwners option'), function () {
        if (this.value !== "" && this.value !== 'Note Owner' && this.value != null) {
            _ownerNames.push((this.value));
        }
    });
    var _noteReviewer = $('#create_noteReviewer').val();
    var _noteStatus = $('#meeting_noteText').val();
    jsonData = JSON.stringify({
        appNo: _appNo,
        seDecision: _seDecision,
        historyNotes: _noteText.replace(/'/g, "''"),
        noteDate: _noteDate,
        noteReviewer: _noteReviewer,
        meetingNotes: _noteStatus.replace(/'/g,"''"),
        ownerList: _ownerNames,
        criticalDate: _criticalDate,
        meetingDate: _meetingDate
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/InsertNoteRecord',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function () {
            $('#modal_createNote').modal('hide');
            $('#toast_success_msg').html('Note Created');
            $('#toast_success').toast('show');
            clearCreateModal();
        },
        error: OnError
    });
}

function updateNoteRecord() {
//updateExistingRows
    var table = $('#example').DataTable();
    var id = $('#note_id').val();
    var _appNo = $("#edit_appNo").val().trim();
    var _seDecision = $('#edit_seDecision').val();
    var _noteText = $('#edit_history_noteText').val();
    var _ownerNames = new Array();
    $.each($('#edit_dd_noteOwners option'), function () {
        if (this.value !== "" && this.value !== 'Note Owner' && this.value != null) {
            _ownerNames.push((this.value));
        }
    });
    //loop to get all owners
    //var _noteDate = new Date().toJSON().slice(0,10).replace(/-/g,'/');
    var _noteDate = new Date().toJSON();
    var _criticalDate = new Date($('#edit_critical_date').val());
    //var _meetingDate = new Date($('#edit_meeting_date').val());
    var _meetingDate = ($('#edit_meeting_date').val() == null || typeof $('#edit_meeting_date').val() === 'undefined' || $('#edit_meeting_date').val() == "" ) ? "" : String(new Date($('#edit_meeting_date').val()).toISOString());
    var _noteReviewer = $('#edit_noteReviewer').val();
    var _noteStatus = $('#edit_meeting_noteText').val();
    if (_appNo == 'null') {
        _appNo = $("#appNo_txt").val().trim()
    }
    var jsonData = JSON.stringify({
        id: id,
        appNo: _appNo,
        seDecision: _seDecision,
        historyNotes: _noteText.replace(/'/g, "''"),
        ownerList: _ownerNames,
        noteDate: _noteDate,
        noteReviewer: _noteReviewer,
        meetingNotes: _noteStatus.replace(/'/g, "''"),
        criticalDate: _criticalDate,
        meetingDate: _meetingDate
    });
    ///here
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/updateNoteRecord',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function () {
            $('#modal_edit').modal('hide');
            $('#toast_success_msg').html('Note Updated');
            $('#toast_success').toast('show');
            clearEditModal();
        },
        error: OnError
    });
}

function deleteRecord(ID) {
//deletes record from anchor
    jsonData = JSON.stringify({
        id: ID
    });
    //shows modal
    $('#modal_confirmDelete').modal('show');
    //if delete button from modal clicked
    $('#btn_delete').on('click', function () {
            $.ajax({
                type: 'post',
                url: '/Services/Dataservice.asmx/deleteRecord',
                async: true,
                data: jsonData,
                contentType: 'application/json; charset=utf-8',
                datatype: 'json',
                success: () => {
                    $('#toast_deleteSucc').toast('show');
                    //still need a refresh button
                },
                error: OnError
            })
    });
}
function GetFilteredQuery() {
    var _appNos = ($('#appNos_txt').val() != "") ? $('#appNos_txt').val().replace(/ /g,"").split(",") : [] ;
    var _basinNos = ($('#basinNos_txt').val() != "")? $('#basinNos_txt').val().replace(/ /g,"").split(","): [] ;
    var _owners = ($('#owner_txt').val() != "")? $('#owner_txt').val().replace(/ /g,"").split(","): [] ;
    var _meetingDates = new Date($('#meetingDates_txt').val()) ? $('#meetingDates_txt').val(): "";
    //var _criticalDates = new Date($('#criticalDates_txt').val()) ? $('#criticalDates_txt').val() : "";

    var jsonData = JSON.stringify({
        appNos: _appNos,
        //criticalDate: _criticalDates,
        meetingDate: _meetingDates,
        basinNo: _basinNos,
        owners: _owners
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/GetFilteredQuery',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            var table = $('#example').DataTable();
            table.clear().rows.add(data.d).draw();
            checkTheme();
        },
        error: OnError
    });
}
function getData() {
//gets data by appNo
    var appNumber = $('#appNo_txt').val();
    var jsonData = JSON.stringify({
        appNo: appNumber
    });
    $.ajax({
        type: 'post',
        url: '/Services/DataService.asmx/getData',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            //console.log(data);
            //add if for null data. lock appno box, show different row until note is created
            var table = $('#example').DataTable();
            table.clear().rows.add(data.d).draw();
            checkTheme();
        },
        error: OnError
    });
}

function selectAllNotes() {
//select all existing rows with note records
    var jsonData = JSON.stringify({});
    $.ajax({
        type: 'post',
        url: '/Services/DataService.asmx/selectAllNotes',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            console.log(JSON.stringify(data));
            var table = $('#example').DataTable();
            table.clear().rows.add(data.d).draw();
            checkTheme();
        },
        error: OnError
    })
}

function editNote(ID) {
//this will use the note id to update fields.
//pseudo: give id, get note values.
    jsonData = JSON.stringify({
        id: ID
    });
    
    //id will return note values from notes table and current owners from owner table.
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/pullEditRecord',
        async: true,
        data: jsonData,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: (data) => {
            console.log(data);
            var i = 0;
            //assign datas here
            var criticalDate = String(new Date(data.d.dCriticalDate).toISOString());
            var meetingDate = (data.d.dMeetingDate == null || data.d.dMeetingDate == undefined || data.d.dMeetingDate === "" || typeof data.d.dMeetingDate === 'undefined' ) ? "" : String(new Date(data.d.dMeetingDate).toISOString());
            console.log(meetingDate);
            $('#note_id').val(data.d.dNoteID);
            $('#edit_appNo').val(data.d.dAppNo);
            $('#edit_noteReviewer').val(data.d.dReviewer);
             currentNoteReviewerName = data.d.dReviewer;
            $('#edit_seDecision').val(data.d.dSeDecision);
            $('#edit_meeting_noteText').val(data.d.dMeetingNotes);
            $('#edit_history_noteText').val(data.d.dNoteText);
            $('#edit_critical_date').val(criticalDate.substring(0, 10));
            $('#edit_meeting_date').val(meetingDate.substring(0, 10));
            $('#edit_dd_noteOwners').html('<option value="" disabled >Note Owners </option>');
            $.each(data.d.dOwnerList, function (d, d) {
                i++;
                $('#edit_dd_noteOwners')
                    .append($("<option ></option>")
                        .attr("value", d)
                        .text(d));
            });
            i = (i > 10) ? 10 : i;
            $("#edit_dd_noteOwners").attr('size', i + 1);
            $('#modal_edit').modal('show');
            popEditOwnersDD();
        },
        error: OnError
    });
}

//will grab and create a list of owners from main source table by app no to search by is the idea
function showOwnerList(app) {
    jsonData = JSON.stringify({
        app: app
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/ShowOwnerList',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            //this will loop to make the list
            for (d in data.d) {
                if (data.d[d].length != 0) {
                    var owner = data.d[d];
                    var selectFunc = `OnClick = "selectByOwner('${owner}')"`;
                    $('#OwnerInfoTable tbody').append(`<tr><td>${data.d[d].OwnerName}</td><td>${data.d[d].OwnerType}</td><td>${data.d[d].OwnersDuty}</td><td>${data.d[d].OwnersDivRate}</td><td>${data.d[d].OwnersAcres}</td></tr>`)
                }
            }
            $('#modal_ownerTitle').text(`Owners: ${app}`);
            $('#modal_owners').modal('show');
        },
        error: OnError
    });
}
function showDetailOwners(noteID) {
    jsonData = JSON.stringify({
        noteID: noteID
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/ShowDetailOwners',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            console.log(data);
            for (d in data.d) {
                console.log('d' + d);
                console.log('data' + data.d[d].OwnerName);
                if (data.d[d].length != 0) {
                    $('#OwnerInfoTable tbody').append(`<tr><td>${data.d[d].OwnerName}</td><td>${data.d[d].OwnerType}</td><td>${data.d[d].OwnersDuty}</td><td>${data.d[d].OwnersDivRate}</td><td>${data.d[d].OwnersAcres}</td></tr>`)
                }
            }
            $('#modal_ownerTitle').text(`Detail Owners (Note ID: ${noteID})`);
            $('#modal_owners').modal('show');
        },
        error: OnError
    });
}
function showParentOwners(app) {
    jsonData = JSON.stringify({
        app: app
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/ShowParentOwners',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            console.log("Owner info!!!"+JSON.stringify(data));
            for (d in data.d) {
                if (data.d[d].length != 0) {
                    $('#OwnerInfoTable tbody').append(`<tr><td>${data.d[d].OwnerName}</td><td>${data.d[d].OwnerType}</td><td>${data.d[d].OwnersDuty}</td><td>${data.d[d].OwnersDivRate}</td><td>${data.d[d].OwnersAcres}</td></tr>`)
                }
            }
            $('#modal_ownerTitle').text(`Detail Owners (Permit Number: ${app})`);
            $('#modal_owners').modal('show');
        },
        error: OnError
    });
}
function selectByOwner(owner) {
//search by owner AND permit number
    console.log('SelectByAppandOwner: ' + owner);
    $('#modal_owners').modal('hide');
    $('#owner_list').empty();
    jsonData = JSON.stringify({
        owner: owner
    });
    $.ajax({
        type: 'post',
        async: true,
        url: '/Services/DataService.asmx/SelectNotesByOwner',
        data: jsonData,
        contentType: 'application/json; charset=utf-8;',
        dataType: 'json',
        success: function (data) {
            console.log('ownerowneorneworiewo' + data.d);
            var table = $('#example').DataTable();
            table.clear().rows.add(data.d).draw();
            checkTheme();
        },
        error: OnError
    });
}

//unfortunately this is doubled because there are seperate owner dropdowns
//you can definitely get rid of this eventually. but you'd have to change the 'change' event to something where exists... maybe change would work. who knows... gill probabl
function popEditOwnersDD() {
    //this grabs owner's currently listen on the app
    var app = $('#edit_appNo').val().replace(' ','');
    jsonData = JSON.stringify({
        app: app
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/showOwnerList',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        datatype: 'json',
        success: function (data) {
            var i = 0;
            $('#edit_dd_ownerList').html('<option value="" disabled selected>Owner: '+app+'</option>');
            $.each(data.d, function (d, d) {
                i++;
                $('#edit_dd_ownerList')
                    .append($("<option></option>")
                        .attr("value", d)
                        .text(d));
            });
            i = (i > 10) ? 10 : i;
            $("#edit_dd_ownerList").attr('size', i+1);
        },
        error: OnError
    });
     $('#editReferenceDateRow').empty();
    getEditReferenceDates();
}

document.getElementById('create_appNo').addEventListener("change", popOwnersDD);
function popOwnersDD() {
    var app = $('#create_appNo').val().replace(' ', '');
    jsonData = JSON.stringify({
        app: app
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/showOwnerList',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        datatype: 'json',
        success: function (data) {
            console.log(data);
            var i = 0;
            $('#dd_ownerList').html('<option value="" disabled selected>Owner: ' + app + '</option>');
            $.each(data.d, function (d, d) {
                i++;
                $('#dd_ownerList')
                    .append($("<option></option>")
                        .attr("value", d)
                        .text(d));
            });
            i = (i > 10) ? 10 : i;
            $("#dd_ownerList").attr('size', i + 1);
        },
        error: OnError
    });
    //dont forget that reference dates are here
    $('#referenceDateRow').empty();
    getReferenceDates();
    getOriginalDueDate(app);
}
function getReferenceDates() {

        $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/getReferenceDates',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            //look at putting a some function to hide the null refdates
            $('#refDateLbl').remove();
            var i = 0;
            //rearrange to 
            //POC, POCFiled, PBUDue, PBU Files
            var dateLabels = ["PBU Filed Date", "PBU Due Date", "POC Filed Date", "POC Due Date"];
            $.each(data.d, function (d, d) {
                console.log(d);
                (d === '1/1/1900') ? d = "N/A" : d = d
                var template = `<td><label class="control-label">${dateLabels[i]}</label>
                            <input style="border-spacing: 10px;" class="form-control" type="text" value="${d}" readonly=""></td>`
                var row = document.getElementById("referenceDateRow");
                var x = row.insertCell(0);
                x.innerHTML = template;
                i++;
            });
        },
        error: OnError
    });
}
//document.getElementsByClassName('edit-icon').addEventListener('click', getEditReferenceDates);
function getEditReferenceDates() {
   
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/getReferenceDates',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            //look at putting a some function to hide the null refdates
            $('#refDateLbl').remove();
            var i = 0;
            //rearrange to 
            //POC, POCFiled, PBUDue, PBU Files
            var dateLabels = ["PBU Filed Date", "PBU Due Date", "POC Filed Date", "POC Due Date"];
            $.each(data.d, function (d, d) {
                console.log(d);
                (d === '1/1/1900') ? d = "N/A" : d = d
                var template = `<td><label class="control-label">${dateLabels[i]}</label>
                            <input style="border-spacing: 10px;" class="form-control" type="text" value="${d}" readonly=""></td>`
                var row = document.getElementById("editReferenceDateRow");
                var x = row.insertCell(0);
                x.innerHTML = template;
                i++;
            });
        },
        error: OnError
    });
}



// #endregion

// #region UI_fun_stuff
//dynamic size for text areas
$('body').on("keydown", "#create_noteText", autoSize);
$('body').on("keydown", "#meeting_noteText", autoSize);
$('body').on("keydown", "#edit_history_noteText", autoSize);
$('body').on("keydown", "#edit_meeting_noteText", autoSize);

function autoSize() {
    var el = this;
    var scrollY = $(window).scrollTop();
   setTimeout(function () {
        el.style.cssText = 'height:auto; padding:0; max-height:100px;';
        // for box-sizing other than "content-box" use:
        // el.style.cssText = '-moz-box-sizing:content-box';
       if (el.scrollHeight <= 200) {
           el.style.cssText = 'height:' + (el.scrollHeight + 8) + 'px';
       } else {
           el.style.cssText = 'height:' + (el.scrollHeight = 200) + 'px';
       }
    }, 0);
}
//footer timer stuff
setInterval(footerTime, 1000);
function footerTime() {
    console.log(new Date().getMonth());
    var currentDate = String(new Date());
    console.log(currentDate);
    //$('#footer').text(currentDate.getMonth() + '/' + currentDate.getDate() + '/' + currentDate.getFullYear() + ' ' + currentDate.getHours() + ':' + currentDate.getMinutes() + ':' +currentDate.getSeconds());
    $('#footer').text(currentDate.substring(0,25));
}
//default stuff
function OnError(response){
    console.log('Error' + JSON.stringify(response));
}
function OnSuccess(response) {
    console.log('Success'+JSON.stringify(response));
}
// #endregion

//#region NotUsingAnymore at least I think...
function ownerFilter(value) {
    if (value !== null || value.length !== 0) {
        return value;
    }
}
function removeDups(data) {
    return data.filter((value, index) => data.indexOf(value) === index);
}
//#endregion
//for Edit modal
$('#swapOwners').click(function () {
    addOwner($('#edit_dd_ownerList').val());
    removeOwner($('#edit_dd_noteOwners').val());
    $('#edit_dd_ownerList option:selected').prop("selected",false);
    $('#edit_dd_noteOwners option:selected').prop("selected",false);
});

function addOwner(selectedOwners) {
    //tomorrow, you need to make this no repeating and connect the drop downs to the database
    var i = $('#edit_dd_noteOwners').attr('size');
    $.each(selectedOwners, function (d) {
        var ifExists = $('#edit_dd_noteOwners option[value="' + selectedOwners[d] + '"]').length;
        if (ifExists === 0) {
            i++;
            $('#edit_dd_noteOwners').append($('<option></option>')
                .attr('value', selectedOwners[d])
                .text(selectedOwners[d]));
        }
    });
    i = (i > 11) ? 11 : i;
    $("#edit_dd_noteOwners").attr('size', i);
}

function removeOwner(selectedOwners) {
    var i = $('#edit_dd_noteOwners').attr('size');
    $.each(selectedOwners, function (d) {
        i--;
        $('#edit_dd_noteOwners option[value="' + selectedOwners[d] + '"]').remove();
    });
    i = (i < 2) ? 2 : i;
    $('#edit_dd_noteOwners').attr('size', i ++);
}
//for create modal
$('#create_swapOwners').click(function () {
    CreateAddOwner($('#dd_ownerList').val());
    CreateRemoveOwner($('#dd_noteOwners').val());
    $('#dd_ownerList option:selected').prop("selected",false);
    $('#dd_noteOwners option:selected').prop("selected",false);
});

function CreateAddOwner(selectedOwners) {
    //tomorrow, you need to make this no repeating and connect the drop downs to the database
    var i = $('#dd_noteOwners').attr('size');
    $.each(selectedOwners, function (d) {
        var ifExists = $('#dd_noteOwners option[value="' + selectedOwners[d] + '"]').length;
        if (ifExists === 0) {
            i++;
            $('#dd_noteOwners').append($('<option></option>')
                .attr('value', selectedOwners[d])
                .text(selectedOwners[d]));
        }
    });
    i = (i > 11) ? 11 : i;
    $("#dd_noteOwners").attr('size', i);
}

function CreateRemoveOwner(selectedOwners) {
    var i = $('#dd_noteOwners').attr('size');
    $.each(selectedOwners, function (d) {
        i--;
        $('#dd_noteOwners option[value="' + selectedOwners[d] + '"]').remove();
    });
    i = (i < 2) ? 2 : i;
    $('#dd_noteOwners').attr('size', i ++);
}

function getDecisions() {
    //here is the idea. We will set a const onload to have all the values. 
    //then we will have a blur or change function to populate the decisions like the owners
   $('#create_seDecision, #edit_seDecision').empty();
   jsonData = JSON.stringify({});
   $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/GetSeDecisions',
        async: true,    
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            $('#create_seDecision, #edit_seDecision').append(new Option("SE Decision", ""));
            $.each(data.d, function (i,item) {
                console.log(item);
                $('#create_seDecision,#edit_seDecision').append(new Option(item.decisionText, item.decisionID));
            });
        },
        error: OnError
    });
}
function getOriginalDueDate(app) {
    jsonData = JSON.stringify({
        appNo: app
    });
    $.ajax({
        type: 'post',
        url: '/Services/Dataservice.asmx/GetOriginalDueDate',
        async: true,
        data: jsonData,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            console.log(data.d);
            var criticalDate = String(new Date(data.d).toISOString());
            $('#create_critical_date').val(criticalDate.substring(0,10));
        },
        error: OnError
    });
}

function clearCreateModal() {
    $('#create_validationForm').clearValidation();
  $('#create_appNo').val('');
    $('#create_seDecision').val('');
    $('#create_noteText').val('');
    $('#meeting_noteText').val('');
    $('#dd_ownerList').val('');
    $('#create_critical_date').val('');
    $('#create_reference_date').val('');
    $('#dd_noteOwners').empty().attr('size', 1).append('<option value disabled>Waiting for Permit No. </option>');
    $('#referenceDateRow').empty();
    $('#create_validationForm').trigger('reset');
    $('#create_validationForm').find(".error").removeClass("border-danger");
    $('#create_validationForm').find(".success").removeClass("border-success");
    $('#meeting_noteText').css('height', "60px");
    $('#create_noteText').css('height', "60px");
    var validator = $('#create_validationForm').validate();
    validator.resetForm();
};
function clearEditModal() {
    $('#edit_critical_date'+
       '#edit_ownerList').val('');
    $('#edit_dd_noteOwners').val('');
    $('#edit_appNo').val('');
    $('#edit_meeting_noteText').val('');
    $('#edit_history_noteText').val('');
    $('#edit_reference_date').val('');
    $('#edit_validationForm').trigger("reset");
    $('#edit_validationForm').find(".error").removeClass("border-danger");
    $('#edit_validationForm').find(".success").removeClass("border-success");
    $('#edit_meeting_noteText').css('height', "60px");
    $('#edit_history_noteText').css('height', "60px");
    var validator = $('#edit_validationForm').validate();
    validator.resetForm();
}

function OnError(response){
    console.log('Error' + JSON.stringify(response));
}
function OnSuccess(response) {
    console.log('Success'+JSON.stringify(response));
}

