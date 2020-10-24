$(document).ready(init);

var rawStandings = "";
var processedStandings = {};
var selectedChar = "";

function init() {
	M.AutoInit();

	$(".tabs").tabs();

	$("#areaStandings").on("keyup", updateImportButton);
	$("#btnImport").on("click", loadStandings);
	$("#playerName").on("change", updateCharacterButton);
	$("#btnSelectChar").on("click", loadMainScreen);
}

function updateImportButton() {
	var areaStandings = $("#areaStandings");
	var enabled = false;
	if (areaStandings.val().length > 0) {
		enabled = true;
	}

	var btnImport = $("#btnImport");
	if (enabled) {
		btnImport.removeClass("disabled");
	}
	else {
		btnImport.addClass("disabled");
	}
}

function loadStandings() {
	$("#rowStandings").addClass("hide");

	var areaStandings = $("#areaStandings");
	rawStandings = areaStandings.val();

	var lines = rawStandings.split("\n");
	for (var i = 0; i < lines.length; i++) {
		var line = lines[i];
		var comps = line.split(",");
		if (comps.length < 6) {
			continue;
		}

		var entry = {};
		entry.Name = comps[0];
		entry.Class = comps[1];
		entry.Rank = comps[2];
		entry.EP = Number(comps[3]);
		entry.GP = Number(comps[4]);
		entry.PR = Number(comps[5]);

		processedStandings[entry.Name] = entry;
	}

	var data = {};
	for(var char in processedStandings) {
		data[char] = null;
	}

	$("#playerName").autocomplete({ data: data });
	$("#rowCharacter").removeClass("hide");
	$("#playerName").focus();
}

function updateCharacterButton() {
	var playerName = $("#playerName");
	var enabled = false;
	if (playerName.val().length > 0) {
		enabled = true;
	}

	var btnSelectChar = $("#btnSelectChar");
	if (enabled) {
		btnSelectChar.removeClass("disabled");
	}
	else {
		btnSelectChar.addClass("disabled");
	}
}

function loadMainScreen() {
	$("#rowCharacter").addClass("hide");
	selectedChar = $("#playerName").val();

	var instance = M.Tabs.getInstance($("#tabsMain"));
	instance.select("gpDiff");

	$("#rowMain").removeClass("hide");
	instance.updateTabIndicator();
}