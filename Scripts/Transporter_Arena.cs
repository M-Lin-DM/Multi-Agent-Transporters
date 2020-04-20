using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.IO;
using MLAgents;
using MLAgents.Sensors;

public class Transporter_Arena : MonoBehaviour
{
    public Transporter_agent agent_p;
    // public Tball tball;
    public GameObject spawnbox;
    public GameObject sinkbox;
    public TextMeshPro cumulative_R;
    public TextMeshPro timeText;
    public TextMeshPro EpisodeText;
    public int NumberAgents;
    float[,] mode_freqs = new float[4,3]; //first index is agent id and second is counts(must be as floats for writecsv function) of the number of steps they were in a certain state namely item-carried = 0,1,2
    
    bool record_mode_tracks = false;
    bool write_mode_freqs = false;
    bool write_rewards = false;
    

    string filename_modefreq = "D:/Unity/Transporters/mode_freq_episode_toss.csv"; 
    string filename_rewards = "D:/Unity/Transporters/episode_rewards_Nagents_16.csv"; 
    string filename_modetrack = "D:/Unity/Transporters/mode_tracks_uniqueID.csv";
    


    private void Start()
    {    
        // WriteArrayToCSV(agent_p.mode_freqs, filename);
        ResetArea();
        // List<int[]> A = new List<int[]>(); // for a potentially quicker fix just write each row separately and attach the id number to the right side.

    }


    public void ResetArea()
    {
        PlaceAgents();
        ClearObjects(GameObject.FindGameObjectsWithTag("Sphere"));
        ClearObjects(GameObject.FindGameObjectsWithTag("Sphere1"));
    }

    void ClearObjects(GameObject[] objects)
    {
        foreach (var j in objects)
        {
            Destroy(j);
        }
    }


    public void UpdateAgentProperties()
    {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
        foreach (GameObject p in agents)
        {
            if (p.GetComponent<Transporter_agent>().has_passed == 1)
            {
                p.GetComponent<Transporter_agent>().has_passed = 0;
                p.GetComponent<Transporter_agent>().item_carried = 0;
            }
            else if (p.GetComponent<Transporter_agent>().has_received == 1)
            {
                p.GetComponent<Transporter_agent>().has_received = 0;
                p.GetComponent<Transporter_agent>().item_carried = 2; 
            }

            CheckViolations(p);
            //INCREMENT COUNTS based on the state the agents are in this frame:
            mode_freqs[p.GetComponent<Transporter_agent>().ID, p.GetComponent<Transporter_agent>().item_carried]  += 1f; //record data on each agent!
            
        }

    }

    public void CheckViolations(GameObject p)
    {
        if (Vector3.Angle(Vector3.up, p.GetComponent<Transform>().up) > 20f)//((Math.Abs(p.GetComponent<Transform>().up.x) > 0.1f) | (Math.Abs(p.GetComponent<Transform>().up.z) > 0.1f))
            {
                // Debug.Log(p.GetComponent<Transform>().forward);
                p.GetComponent<Transform>().LookAt(new Vector3(p.GetComponent<Transform>().forward.x, 0, p.GetComponent<Transform>().forward.z), Vector3.up);
            } 
        // else if (p.GetComponent<Transform>().childCount > 3)
        //     {
        //         // Debug.Log("too many balls carried.." + p.GetComponent<Transform>().childCount.ToString());
        //         p.GetComponent<Transporter_agent>().OnEpisodeBegin();
        //         // 
        //     } 

    }

    private void FixedUpdate()
    {
        float tt = Time.fixedTime;
        // Update the cumulative reward text
        // GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
        // float cum_R = 0;
        // foreach (GameObject p in agents)
        // {
        //     cum_R += p.GetComponent<Transporter_agent>().GetCumulativeReward();
            
        // }
        // Debug.Log(cum_R);
        // cumulative_R.text = agent_p.GetCumulativeReward().ToString("0.0");
        // cumulative_R.text = cum_R.ToString("0.00");

        timeText.text = "Steps: " + Academy.Instance.StepCount.ToString("0"); //STEPS
        EpisodeText.text = "Episode: " + Math.Floor((double)Academy.Instance.TotalStepCount / agent_p.GetComponent<Transporter_agent>().maxStep).ToString("0"); //episode counter
        // timeText.text = agent_p.StepCount.ToString("0");
        // timeText.text =tt.ToString("0.0");
        UpdateAgentProperties();


//  write data to csv:
        if ((Academy.Instance.StepCount + 1) % agent_p.GetComponent<Transporter_agent>().maxStep == 0) //(the "+1" and the parentheses are both important. you will get a cum_R=0 without +1 (we evaluate rewards at the last step before the episodes resets. +0 would mean the first step of new episode)
        {
            // Debug.Log("Writing to CSV: " + string.Join(",", mode_freqs));
            if (write_mode_freqs)
            {
                WriteArrayToCSV(mode_freqs, filename_modefreq, false);
            }

            if (write_rewards)
            {
                GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
                float cum_R = 0;
                foreach (GameObject p in agents)
                {
                    cum_R += p.GetComponent<Transporter_agent>().GetCumulativeReward();
                    // Debug.Log(cum_R);
                }
                float[,] episode_data = new float[1,2] {{Convert.ToSingle(Math.Floor((double)Academy.Instance.TotalStepCount / agent_p.GetComponent<Transporter_agent>().maxStep)), cum_R}}; //episode number, cumulative reward of all agents this episode
                WriteArrayToCSV(episode_data, filename_rewards, true);
            }
        }

        if (record_mode_tracks)
        {
            GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
            float[,] items_carried_by_agents = new float[1,1 + agents.GetLength(0)];
            items_carried_by_agents[0,0] = (float)Academy.Instance.StepCount;
            foreach (GameObject p in agents)
            {
                items_carried_by_agents[0, p.GetComponent<Transporter_agent>().ID + 1] = p.GetComponent<Transporter_agent>().item_carried; //[stepcount, item carried by agent 0, by 1, by 2, ...]
            }
            WriteArrayToCSV(items_carried_by_agents, filename_modetrack, true);
        }

        
    }

    public void PlaceAgents()
    {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
        foreach (GameObject p in agents)
        {
                p.transform.SetParent(transform);
                p.transform.position = new Vector3(UnityEngine.Random.Range(-20, 20), 0.5f, UnityEngine.Random.Range(-20, 20));           
                p.transform.rotation = Quaternion.Euler(new Vector3(0f, UnityEngine.Random.Range(0, 360), 0f));
                // Debug.Log("placed agents");
        }

    }

    public void WriteArrayToCSV(float[,] data, string file, bool append)
    {
        if (append)
        {
        // string filename = "D:/ml-agents-release-0.15.1/ml-agents-release-0.15.1/Project/Assets/ML-Agents/Examples/Thermoregulators/extracted_data/group_mean_Tp_RTg_2.csv";
            using (var writer = new StreamWriter(file, append: true))
            {
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    IEnumerable row = ExtractRow(data, i);     //get Enumerator representing/generating the ith row
                    float[] rowarray = row.Cast<float>().ToArray(); // cast it as an array of ints
                    string row_string = string.Join(",", rowarray); //convert array to string separated by commas
                    writer.WriteLine(row_string);  //write each line to csv
                }
            
            }
        }
        else
        {
            using (var writer = new StreamWriter(file))
            {
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    IEnumerable row = ExtractRow(data, i);     //get Enumerator representing/generating the ith row
                    float[] rowarray = row.Cast<float>().ToArray(); // cast it as an array of ints
                    string row_string = string.Join(",", rowarray); //convert array to string separated by commas
                    writer.WriteLine(row_string);  //write each line to csv
                }
            
            }

        }
    }

    public static IEnumerable ExtractRow(float[,] array, int row) //this is an extention method
    {
        for (var i = 0; i < array.GetLength(1); i++)
        {
            yield return array[row, i];
        }
    }
}

//make a section using EndEpisode() when only green balls captured