// TEMPORARY FILE - Instructions:
// 1. Open GameManager.cs
// 2. Find lines 236-242 in HandleFootsteps() method
// 3. Replace them with this fixed code:

/*
                sfxSource.PlayOneShot(clip, vol);
            }

            // Emit environmental noise for sanity system (always, regardless of audio)
            float noiseIntensity = isCrouched ? crouchNoiseIntensity : (running ? runNoiseIntensity : walkNoiseIntensity);
            EnvironmentalNoiseEmitter.OnEnvironmentalNoise?.Invoke(noiseIntensity);

            // Ajuste simple con la velocidad para que aumente la cadencia si vas más rápido
*/

// THE BUG: Lines 238-240 are INSIDE the "if (pool != null...)" block
// THE FIX: Move them OUTSIDE the block so they always execute when stepTimer triggers

// Delete this file after fixing GameManager.cs
