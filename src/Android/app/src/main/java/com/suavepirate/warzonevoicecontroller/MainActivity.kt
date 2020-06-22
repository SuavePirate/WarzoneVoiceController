package com.suavepirate.warzonevoicecontroller

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import android.view.View
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.github.kittinunf.fuel.Fuel
import com.github.kittinunf.fuel.android.BuildConfig.VERSION_CODE
import com.github.kittinunf.fuel.gson.jsonBody
import com.github.kittinunf.fuel.gson.responseObject
import com.github.kittinunf.fuel.httpPost
import com.google.android.exoplayer2.core.BuildConfig.VERSION_CODE
import com.google.android.material.snackbar.Snackbar
import com.suavepirate.warzonevoicecontroller.BuildConfig.VERSION_CODE
import io.spokestack.spokestack.BuildConfig
import io.spokestack.spokestack.OnSpeechEventListener
import io.spokestack.spokestack.SpeechContext
import io.spokestack.spokestack.SpeechPipeline
import io.spokestack.spokestack.nlu.NLUResult
import io.spokestack.spokestack.nlu.TraceListener
import io.spokestack.spokestack.nlu.tensorflow.TensorflowNLU
import io.spokestack.spokestack.tts.SynthesisRequest
import io.spokestack.spokestack.tts.TTSManager
import io.spokestack.spokestack.util.EventTracer
import kotlinx.android.synthetic.main.activity_main.*
import kotlinx.android.synthetic.main.content_main.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.File
import java.io.FileOutputStream
import java.io.IOException
import java.io.InputStream
import java.util.*


class MainActivity : AppCompatActivity(), OnSpeechEventListener, TraceListener {
    private var pipeline: SpeechPipeline? = null
    private var tts: TTSManager? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        checkForModels()

        pipeline = SpeechPipeline.Builder()
            .useProfile("io.spokestack.spokestack.profile.VADTriggerAndroidASR")
            .setAndroidContext(applicationContext)
            .addOnSpeechEventListener(this)
            .build()
        tts = TTSManager.Builder()
            .setTTSServiceClass("io.spokestack.spokestack.tts.SpokestackTTSService")
            .setOutputClass("io.spokestack.spokestack.tts.SpokestackTTSOutput")
            .setProperty("spokestack-id", "3b40def8-f77e-42f8-8d5c-47a8fd7e4797")
            .setProperty(
                "spokestack-secret",
                "9DAC2C5AA917752AB1854B47DE1990560203394AF5716E157EDD57C7DAD99949"
            )
            .setAndroidContext(applicationContext)
            .setLifecycle(lifecycle)
            .build()

        fab.setOnClickListener { view ->
            tts?.stopPlayback()
            when {
                ContextCompat.checkSelfPermission(
                    this,
                    Manifest.permission.RECORD_AUDIO
                ) == PackageManager.PERMISSION_GRANTED -> {
                    // You can use the API that requires the permission.
                    if (pipeline?.isRunning == true) {
                        pipeline?.stop()
                        fab.setImageResource(android.R.drawable.ic_media_play);
                        Snackbar.make(view, "No longer listening", Snackbar.LENGTH_LONG)
                            .setAction("Action", null).show()
                    } else {

                        pipeline?.start()
                        fab.setImageResource(android.R.drawable.ic_media_pause);
                        Snackbar.make(view, "Now listening", Snackbar.LENGTH_LONG)
                            .setAction("Action", null).show()
                    }
                }
                else -> {
                    // You can directly ask for the permission
                    ActivityCompat
                        .requestPermissions(
                            this,
                            Array<String>(1) { _ -> Manifest.permission.RECORD_AUDIO },
                            1
                        );
                }
            }

        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
    }

    override fun onEvent(event: SpeechContext.Event?, context: SpeechContext?) {
        when (event) {
            SpeechContext.Event.ACTIVATE -> println("ACTIVATED")
            SpeechContext.Event.DEACTIVATE -> println("DEACTIVATED")
            SpeechContext.Event.RECOGNIZE -> context?.let { handleSpeech(it.transcript) }
            SpeechContext.Event.TIMEOUT -> println("TIMEOUT")
            SpeechContext.Event.ERROR -> context?.let { println("ERROR: ${it.error}") }
            else -> {
                // do nothing
            }
        }
    }

    private fun handleSpeech(transcript: String) {
        pipeline?.stop()
        pipeline?.start()
        val nlu = TensorflowNLU.Builder()
            .setProperty("nlu-model-path", "$cacheDir/nlu.tflite")
            .setProperty("nlu-metadata-path", "$cacheDir/metadata.json")
            .setProperty("wordpiece-vocab-path", "$cacheDir/vocab.txt")
            .addTraceListener(this)
            .build()

        GlobalScope.launch(Dispatchers.Default) {
            nlu?.let {
                val result = it.classify(transcript).get()
                val intent = result.intent
                withContext(Dispatchers.Main) {
                    Snackbar.make(fab, result.intent, Snackbar.LENGTH_LONG)
                        .setAction("Action", null).show()
                    commandLogTextView.text = commandLogTextView.text.toString() + "\n$intent"
                    commandScrollView.fullScroll(View.FOCUS_DOWN)
                }
                if(!intent.startsWith("AMAZON")) {
                    "https://warzonevoicecontroller.azurewebsites.net/api/command/$intent".httpPost()
                        .response()
                }
            }
        }
    }

    private fun checkForModels() {
        // PREF_NAME and VERSION_KEY are static Strings set at the top of the file;
        // we want PREF_NAME to uniquely refer to our app, and VERSION_KEY to be
        // unique within the app itself
        if (!modelsCached()) {
            decompressModels()
        } else {
            val currentVersionCode = com.suavepirate.warzonevoicecontroller.BuildConfig.VERSION_CODE
            val prefs = getSharedPreferences("com.suavepirate.warzonevoicecontroller", Context.MODE_PRIVATE)
            val savedVersionCode = prefs.getInt("VERSION_KEY", -1)
            if (currentVersionCode != savedVersionCode) {
                decompressModels()

                // Update the shared preferences with the current version code
                prefs.edit().putInt("VERSION_KEY", currentVersionCode).apply()
            }
        }
    }

    private fun modelsCached(): Boolean {
        val nluName = "nlu.tflite"
        val nluFile = File("$cacheDir/$nluName")
        return nluFile.exists()
    }

    private fun decompressModels() {
        try {
            cacheAsset("nlu.tflite")
            cacheAsset("metadata.json")
            cacheAsset("vocab.txt")
        } catch (e: IOException) {
            Log.e("WZ", "Unable to cache NLU data", e)
        }
    }

    @Throws(IOException::class)
    private fun cacheAsset(assetName: String) {
        val assetFile = File("$cacheDir/$assetName")
        val inputStream: InputStream = assets.open(assetName)
        val size: Int = inputStream.available()
        val buffer = ByteArray(size)
        inputStream.read(buffer)
        inputStream.close()
        val fos = FileOutputStream(assetFile)
        fos.write(buffer)
        fos.close()
    }

    override fun onTrace(level: EventTracer.Level?, message: String?) {
        Log.w("WZ", message);
    }
}
