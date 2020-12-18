using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleExpandability : MonoBehaviour
{
    public GameObject[] expanders;
    public GameObject[] collapsers;
    public GameObject[] panels;

    public void ConvertToExpandable(bool doExpand) {
        foreach (GameObject collapser in collapsers) {
            collapser.SetActive(doExpand);
        }
        foreach (GameObject expander in expanders) {
            expander.SetActive(!doExpand);
        }
        foreach (GameObject panel in panels) {
            panel.SetActive(doExpand);
        }
    }

    public void ConvertToAlwaysExpanded() {
        foreach (GameObject collapser in collapsers) {
            collapser.SetActive(false);
        }
        foreach (GameObject expander in expanders) {
            expander.SetActive(false);
        }
        foreach (GameObject panel in panels) {
            panel.SetActive(true);
        }
    }
}
