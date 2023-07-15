 if ("webkitSpeechRecognition" in window) {
  // Initialize webkitSpeechRecognition
  let speechRecognition = new webkitSpeechRecognition();
  let isNewLine = false;
  let l2r = true; //left 2 right
  let isStarted = false;
  // String for the Final Transcript
  let final_transcript = "";

  // Set the properties for the Speech Recognition object
  speechRecognition.continuous = true;
  speechRecognition.interimResults = true;
  speechRecognition.lang = document.querySelector("#select_dialect").value;

  // Callback Function for the onStart Event
  speechRecognition.onstart = () => {
    // Show the Status Element
    document.querySelector("#status").style.display = "block";
  };
  speechRecognition.onerror = () => {
    // Hide the Status Element
    document.querySelector("#status").style.display = "none";
  };
  speechRecognition.onend = () => {
    // Hide the Status Element
    document.querySelector("#status").style.display = "none";
  };

  speechRecognition.onresult = (event) => {
    // Create the interim transcript string locally because we don't want it to persist like final transcript
    let interim_transcript = "";

    // Loop through the results from the speech recognition object.
    for (let i = event.resultIndex; i < event.results.length; ++i) {
      // If the result item is Final, add it to Final Transcript, Else add it to Interim transcript
      if (event.results[i].isFinal) {
        final_transcript += event.results[i][0].transcript;
      } else {
        interim_transcript += event.results[i][0].transcript;
      }
    }

    if(isNewLine) { isNewLine = false; final_transcript += "<br/>"; }
    // Set the Final transcript and Interim transcript.
    document.querySelector("#final").innerHTML = final_transcript;
    document.querySelector("#interim").innerHTML = interim_transcript;
  };

  // Set the onClick property of the start button
  document.querySelector("#start").onclick = () => {
      if(!isStarted){
        //reset the lang
        speechRecognition.lang = document.querySelector("#select_dialect").value;
        // Start the Speech Recognition
        speechRecognition.start();
        document.querySelector("#start").disabled = true;
        document.querySelector("#stop").disabled = false;
        isStarted = true;
      }
  };
  // Set the onClick property of the stop button
  document.querySelector("#stop").onclick = () => {
      if(isStarted){
        // Stop the Speech Recognition
        speechRecognition.stop();
        document.querySelector("#start").disabled = false;
        document.querySelector("#stop").disabled = true;
        isStarted = false;
      }
  };
  document.querySelector("#clear").onclick = () => {
    // clear the Speech Recognition
    final_transcript = "";
    interim_transcript = "";
    document.querySelector("#final").innerHTML = "";
  };
  document.querySelector("#new-line").onclick = () => {
    isNewLine = true;
  };
  document.querySelector("#left2right").onclick = () => {
      l2r = !l2r;
      if(l2r){
        document.querySelector("#left2right").innerHTML = "Left 2 Right";
        document.querySelector("#transcript").classList.remove("text-end");
        document.querySelector("#transcript").classList.add("text-start");
      }
      else{
        document.querySelector("#left2right").innerHTML = "Right 2 Left";
        document.querySelector("#transcript").classList.remove("text-start");
        document.querySelector("#transcript").classList.add("text-end");
      }    
  };
  document.querySelector("#copy").onclick = () => {
    // copy the Speech Recognition
    let txt = final_transcript.replaceAll("<br/>","\n");
    navigator.clipboard.writeText(txt);
  };
} else {
  console.log("Speech Recognition Not Available");
}