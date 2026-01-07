// 2D RPG voice pitch detector by 林龙
// Global variable for Unity communication
global int trendResult;  // 0=NONE, 1=ASCENDING, 2=DESCENDING, 3=STABLE
0 => trendResult;
global int isActive; 
1 => isActive;
adc => Gain input => blackhole;

// Parameters
1024 => int BUFFER_SIZE;
100::ms => dur ANALYSIS_INTERVAL;
1000::ms => dur TREND_ANALYSIS_DURATION;

// Thresholds - ADJUST THESE AFTER TESTING
0.02 => float AMPLITUDE_THRESHOLD;
80.0 => float MIN_FREQ;
1000.0 => float MAX_FREQ;
5.0 => float MIN_PITCH_STEP;

float samples[BUFFER_SIZE];
float pitchHistory[15];
0 => int pitchCount;

// Autocorrelation function for pitch detection
fun float detectPitch(float samples[], int size)
{
    float maxCorrelation;
    int bestLag;
    (second / samp) $ int => int sampleRate;
    (sampleRate / MAX_FREQ) $ int => int minLag;
    (sampleRate / MIN_FREQ) $ int => int maxLag;
    
    if (maxLag > size / 2) size / 2 => maxLag;
    
    -1.0 => maxCorrelation;
    0 => bestLag;
    
    for (minLag => int lag; lag < maxLag; lag++) {
        0.0 => float correlation;
        0.0 => float energy;
        
        for (0 => int i; i < size - lag; i++) {
            samples[i] * samples[i + lag] +=> correlation;
            samples[i] * samples[i] +=> energy;
        }
        
        if (energy > 0.0) {
            correlation / energy => correlation;
        }
        
        if (correlation > maxCorrelation) {
            correlation => maxCorrelation;
            lag => bestLag;
        }
    }
    
    if (maxCorrelation > 0.3 && bestLag > 0) {
        return (second / samp) / bestLag;
    }
    return 0.0;
}

// Function to calculate RMS
fun float calculateRMS(float samples[], int size)
{
    0.0 => float sum;
    for (0 => int i; i < size; i++) {
        samples[i] * samples[i] +=> sum;
    }
    return Math.sqrt(sum / size);
}

// Function to analyze pitch trend
fun string analyzePitchTrend(float history[], int count)
{
    if (count < 3) return "NONE";
    
    0 => int ascendingSteps;
    0 => int descendingSteps;
    
    for (0 => int i; i < count - 1; i++) {
        history[i+1] - history[i] => float diff;
        
        if (diff > MIN_PITCH_STEP) {
            ascendingSteps++;
        }
        else if (diff < -MIN_PITCH_STEP) {
            descendingSteps++;
        }
    }
    
    if (ascendingSteps > descendingSteps) {
        return "ASCENDING";
    }
    else if (descendingSteps > ascendingSteps) {
        return "DESCENDING";
    }
    else {
        return "STABLE";
    }
}

// Main analysis loop
<<< "==================================" >>>;
<<< "2D RPG Voice Pitch Detector READY" >>>;
<<< "==================================" >>>;
<<< "Listening for voice input..." >>>;
<<< "" >>>;

while (true){
  if (isActive == 1) {
    // Collect samples
    for (0 => int i; i < BUFFER_SIZE; i++) {
        input.last() => samples[i];
        1::samp => now;
    }
    
    calculateRMS(samples, BUFFER_SIZE) => float rms;
    
    // Check if signal is loud enough
    if (rms > AMPLITUDE_THRESHOLD) {
        detectPitch(samples, BUFFER_SIZE) => float frequency;
        
        if (frequency >= MIN_FREQ && frequency <= MAX_FREQ) {
            <<< ">>> Voice detected! Analyzing for 1.5 seconds..." >>>;
            0 => pitchCount;
            
            // Collect pitch data for 1500ms
            now => time analysisStart;
            while (now - analysisStart < TREND_ANALYSIS_DURATION) {
                for (0 => int i; i < BUFFER_SIZE; i++) {
                    input.last() => samples[i];
                    1::samp => now;
                }
                
                calculateRMS(samples, BUFFER_SIZE) => float currentRMS;
                
                if (currentRMS > AMPLITUDE_THRESHOLD) {
                    detectPitch(samples, BUFFER_SIZE) => float currentFreq;
                    
                    if (currentFreq >= MIN_FREQ && currentFreq <= MAX_FREQ) {
                        currentFreq => pitchHistory[pitchCount];
                        pitchCount++;
                        <<< "Frequency:", currentFreq, "Hz | Volume:", currentRMS >>>;
                    }
                }
                ANALYSIS_INTERVAL => now;
            }
            
            // Analyze and broadcast result
            analyzePitchTrend(pitchHistory, pitchCount) => string trend;
            
            <<< "" >>>;
            <<< "=============================" >>>;
            <<< "RESULT:", trend >>>;
            <<< "=============================" >>>;
            
            // Set the global variable for Unity
            if (trend == "ASCENDING") {
                1 => trendResult;
            }
            else if (trend == "DESCENDING") {
                2 => trendResult;
            }
            else if (trend == "STABLE") {
                3 => trendResult;
            }
            else {
                0 => trendResult;
            }
            
            <<< "" >>>;
            <<< "Listening for voice input..." >>>;
            <<< "" >>>;
        }
    }
   
 }
    else {
        0 => trendResult;
    }   
    ANALYSIS_INTERVAL => now;
}
