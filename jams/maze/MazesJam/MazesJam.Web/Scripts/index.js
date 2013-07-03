// directional constants ('cuz it reads better)
var dir = { N : 1, S : 2, E : 4, W : 8, All : 15 };

// sample maze definitions... useful for testing
var samples = {
  built   : [ [0,2,4,0,2,6,2,0,4,0], 
              [6,0,6,4,0,6,2,6,0,2],
              [2,2,4,2,2,4,0,4,2,0], 
              [2,2,4,2,2,0,6,4,0,2],
              [4,0,0,0,2,4,4,0,4,0], 
              [2,4,6,6,2,6,2,6,2,2],
              [0,4,4,4,4,4,0,2,0,2], 
              [4,2,6,2,0,6,0,6,0,2],
              [4,4,4,4,0,2,0,6,0,0], 
              [0,0,0,0,0,4,0,4,4,0] ],
  carved  : [ [6, 8,10,10,14, 8,12,14,12,10],
              [7, 9,11,15,11,11,15,11,13,11],
              [7, 9,13,11,13,13, 9, 9,15, 9],
              [7, 9,13,15, 9,15,15,11,13,11],
              [5,11,15,13, 9, 9, 9,11,15, 9],
              [3,15,11,15, 9,15,11,11,11,11],
              [5,11,13,13,13, 9,13, 9,13, 9],
              [5,13,11,13,11,11,11,13,13,11],
              [3,15,13,11,11,11,11,11,11,11],
              [5,13, 9,13,13,13,13,13,13, 9] ]
};

function builtToCarved(built){
  // get basic dimensions from definition
  var height = built.length;
  var width  = d3.max(built,function(a){return a.length;});
  // based on the walls in a cell, calculate the passages
  function translate(x,y,d) {
    var cell = d;
    // normalize cell by adding outer-edge walls
    if(y===0)       { cell |= dir.N; } 
    if(x===0)       { cell |= dir.W; }
    if(y+1>=height) { cell |= dir.S; }
    if(x+1>=width)  { cell |= dir.E; }
    // the difference between all possible passages (out of a cell) and
    // the total number of cell walls is the actual number of passages 
    return dir.All - cell;
  }
  // blast through definition, translating each cell in turn
  return built.map(function(row ,y){
    return row.map(function(cell,x){return translate(x,y,cell);});
  });
}

function loadMazeDef(maze){
  $("#mazeTextual").val(JSON.stringify(maze));
}

function renderMaze(maze,cellSize){
  // reset table to be empty
  $("#mazeVisual").empty("tr");
  // get basic dimensions from definition
  var height  = maze.length;
  var width   = d3.max(maze,function(a){return a.length;});
  // prepare table
  var table   = d3.select("#mazeVisual")
                  .attr("width" ,cellSize * width )
                  .attr("height",cellSize * height);
  // bind rows to table
  var rows    = table.selectAll("tr")
                     .data(maze)
                     .enter()
                     .append("tr");
  // bind cells to rows
  var cells   = rows.selectAll("td")
                    .data(function(d){return d;})
                    .enter()
                    .append("td");
  // (optionally) display cell value
  if((cellSize > 18) || (cellSize > $("label[for=cellSize]").height())){ 
    //TODO: replace `18` with a more practical (calculated) metric
    cells.text(function(d){ return String(d); }); 
  }
  // carved passages based on bitmask values
  cells.classed("carveN",function(d){return (d & dir.N);}) 
       .classed("carveS",function(d){return (d & dir.S);}) 
       .classed("carveE",function(d){return (d & dir.E);}) 
       .classed("carveW",function(d){return (d & dir.W);});
}

function fetchMazeImage(mazeText,cellSize){
  var render = $.post($("#mazeRender").attr('action') // service URL
                     ,{ maze : mazeText, cellSize : cellSize }
                     ,null //NOTE: success callback defined later
                     //NOTE: by telling the service we want 'text', we get
                     //      a Base64-encoded blob, which is just about 
                     //      the only way to set an in-memory image     
                     ,'text')
  // failure
  render.fail(function(_,status){ 
    console.log("fetchMazeImage reported: "+ status);
    $("#mazeImageFig").hide();
    alert("Unable to get image from server.");
  });
  // success
  render.done(function(imageData){
    console.log("fetchMazeImage reported: success");
    // use data URI to load image as Base64-encoded blob
    var imgSrc = "data:image/png;base64," + imageData;
    $("#mazeImage").attr("src",imgSrc);
    $("#mazeImageFig").show();
  });
}

function shouldCallServer(){
  return $("#callServer:checked").val() !== undefined;
}

function updateDisplay(){
  try { 
    var mazeText = $("#mazeTextual").val();
    var mazeJSON = $.parseJSON(mazeText);
    var cellSize = parseInt($("#cellSize").val()); //TODO: more robust
    if(!cellSize) {
      $("#cellSize").val(30);
      throw new Error("Cell Size MUST be an integral number");
    }
    renderMaze($.makeArray(mazeJSON),cellSize);
    $("#mazeTableFig").show();
    if(shouldCallServer()) fetchMazeImage(mazeText,cellSize);
  } catch(err) { alert(err); }
}

function initDisplay(){
  $("#mazeImageFig").hide(); 
  $("#mazeTableFig").hide();
}

$(document).ready(function(){
  $("#callServer").change(function() {
    var imgFig   = $("#mazeImageFig");
    var isHidden = (imgFig.css("display") === "none");
    if(!shouldCallServer() && !isHidden) imgFig.hide();
  });
  
  $("#loadDefaultCarved").click(function(){loadMazeDef(samples.carved);});
  $("#loadDefaultBuilt" ).click(function(){loadMazeDef(samples.built );});

  $("#builtToCarved").click(function(){
    try {
      var mazeJSON = $.parseJSON($("#mazeTextual").val());
      loadMazeDef(builtToCarved($.makeArray(mazeJSON)));
    } catch(err) { alert(err); }
  });
  
  $("#mazeRender").submit(function(){ updateDisplay(); 
                                      return false; });

  initDisplay();
});
