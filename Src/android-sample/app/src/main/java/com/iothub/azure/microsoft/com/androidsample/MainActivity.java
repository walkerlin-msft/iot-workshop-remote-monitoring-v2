package com.iothub.azure.microsoft.com.androidsample;

import android.content.Context;
import android.content.DialogInterface;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.SeekBar;
import android.widget.TextView;
import android.widget.Toast;

import com.microsoft.azure.sdk.iot.device.IotHubClientProtocol;

import java.util.ArrayList;

import static android.content.DialogInterface.BUTTON_POSITIVE;

public class MainActivity extends AppCompatActivity {

    private static final String TAG = "MainActivity";

    private static final String defaultConnectionString = "<Please put your device connection string here>";
    public static String connString;

    //public static IotHubClientProtocol protocol = IotHubClientProtocol.HTTPS;
    public static IotHubClientProtocol protocol = IotHubClientProtocol.MQTT;

    private MainActivity mainActivity;
    Context context;
    Button btnSendMessage;
    TextView sendMessage;
    TextView receiveMessage;
    boolean isStartingToSend = false;
    private SendMessageAsyncTask sendTask;
    TextView seekbarTemperatureText;
    TextView seekbarHumidityText;
    SeekBar seekbarTemperature;
    SeekBar seekbarHumidity;
    Toolbar toolbarMain;
    int temperature;
    double humidity;
    String deviceId;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        this.context = this;
        this.mainActivity = this;
        this.connString = getPreferencesConnectionString();
        this.deviceId = getDeviceId(this.connString);
        findAllViews();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    private void findAllViews() {
        toolbarMain = (Toolbar) findViewById(R.id.toolbar);
        toolbarMain.setTitle(getString(R.string.app_name));
        setSupportActionBar(toolbarMain);
        toolbarMain.setOnMenuItemClickListener(onMenuItemClick);

        btnSendMessage = (Button) findViewById(R.id.btnSend);
        sendMessage = (TextView) findViewById(R.id.textSend);
        receiveMessage = (TextView) findViewById(R.id.textReceive);
        seekbarTemperatureText = (TextView) findViewById(R.id.seekbarTemperatureText);
        seekbarHumidityText = (TextView) findViewById(R.id.seekbarHumidityText);
        seekbarTemperature = (SeekBar) findViewById(R.id.seekbarTemperature);
        seekbarTemperature.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
            @Override
            public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
                saveAndUpdateTemperature(progress);
            }

            @Override
            public void onStartTrackingTouch(SeekBar seekBar) {

            }

            @Override
            public void onStopTrackingTouch(SeekBar seekBar) {
                saveAndUpdateTemperature(seekBar.getProgress());
            }
        });

        saveAndUpdateTemperature(seekbarTemperature.getProgress());

        seekbarHumidity = (SeekBar) findViewById(R.id.seekbarHumidity);
        seekbarHumidity.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
            @Override
            public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {

                double humidity = (double)progress;
                saveAndUpdateHumidity(humidity);
            }

            @Override
            public void onStartTrackingTouch(SeekBar seekBar) {

            }

            @Override
            public void onStopTrackingTouch(SeekBar seekBar) {
                double humidity = (double)seekBar.getProgress();
                saveAndUpdateHumidity(humidity);
            }
        });

        getAndUpdateHumidity();
    }

    private DialogInterface.OnClickListener onClickListener = new DialogInterface.OnClickListener() {
        @Override
        public void onClick(DialogInterface dialogInterface, int i) {

            if(i == BUTTON_POSITIVE) {
                Log.d("SettingsDialog", "setPositiveButton");

                EditText edit = (EditText) ((AlertDialog) dialogInterface).findViewById(R.id.edittext_connectionstring);
                connString = edit.getText().toString();
                updatePreferencesConnectionString(connString);
            }
        }
    };

    private Toolbar.OnMenuItemClickListener onMenuItemClick = new Toolbar.OnMenuItemClickListener() {
        @Override
        public boolean onMenuItemClick(MenuItem item) {
            switch (item.getItemId()) {
                case R.id.action_settings:
                    Log.d(TAG, "action_settings");
                    SettingsDialog dialog = new SettingsDialog();
                    dialog.showDialog(mainActivity, onClickListener);
                    break;
            }

            return true;
        }
    };

    private String getDeviceId(String connectionString) {
        try {
            String[] fields = connectionString.split(";");

            return fields[1].substring(fields[1].indexOf("=") + 1);
        } catch (Exception e) {
            return "DEVICE_NOT_FOUND";
        }
    }

    private String getPreferencesConnectionString() {
        SharedPreferences sharedPref = this.getPreferences(Context.MODE_PRIVATE);
        return sharedPref.getString("ConnectionString", defaultConnectionString);
    }

    private void updatePreferencesConnectionString(String cs) {
        SharedPreferences sharedPref = this.getPreferences(Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPref.edit();
        editor.putString("ConnectionString", cs);
        editor.commit();
    }

    private void saveAndUpdateTemperature(int temperature) {
        seekbarTemperatureText.setText(getResources().getString(R.string.temperature)+": "+temperature);
        setTemperature(temperature);
    }

    private void saveAndUpdateHumidity(double humidity) {
        seekbarHumidityText.setText(getResources().getString(R.string.humidity)+": "+ humidity);
        setHumidity(humidity);
    }

    private void getAndUpdateHumidity() {
        double humidity = (double) seekbarHumidity.getProgress();
        saveAndUpdateHumidity(humidity);
    }

    @Override
    protected void onDestroy() {
        stopSendMessage();

        super.onDestroy();
    }

    public void btnSendOnClick(View v) {
        if (!isStartingToSend)
            sendMessage();
        else
            stopSendMessage();
    }

    private void sendMessage() {
        isStartingToSend = true;

        sendTask = new SendMessageAsyncTask(this, new SendMessageAsyncTask.ProgressUpdateCallback() {
            @Override
            public void callback(ArrayList<String> values) {
                String key = values.get(0);
                String value = values.get(1);
                if (key.equals("SEND"))
                    sendMessage.setText(value);
                else if (key.equals("RECEIVE")) {
                    receiveMessage.setText(value);

                    Toast.makeText(getContext(),
                            value,
                            Toast.LENGTH_SHORT).show();
                }
            }
        });
        sendTask.execute();

        btnSendMessage.setText("Stop");
    }

    private Context getContext() {
        return this.context;
    }

    private void stopSendMessage() {
        isStartingToSend = false;

        if (sendTask != null && sendTask.getStatus() != AsyncTask.Status.FINISHED)
            sendTask.cancel(true);

        sendMessage.setText(sendMessage.getText() + "(stopped)");
        btnSendMessage.setText("Run");
    }

    public double getHumidity() {
        return humidity;
    }

    public void setHumidity(double humidity) {
        this.humidity = humidity;
    }

    public int getTemperature() {
        return temperature;
    }

    public void setTemperature(int temperature) {
        this.temperature = temperature;
    }

    public String getDeviceId() {
        return deviceId;
    }
}
