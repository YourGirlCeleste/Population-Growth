using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Organism{

    public string name;
    public string type = "NONE";
    public int health = 2;
    public int generation;
    
    public Organism parentOne = null;
    public Organism parentTwo = null;

    public Organism assignedPartner = null;
    public int pairNum = 0;

    public List<Organism> familyHistory = new List<Organism>();
}

public enum SelectionType{

    random,
    set
}

public class SimulationManager : MonoBehaviour{

    [Header("Organisms")]
    Dictionary<int, List<Organism>> activeOrganisms = new Dictionary<int, List<Organism>>();
    Dictionary<int, List<Organism>> availableOrganisms = new Dictionary<int, List<Organism>>();
    Dictionary<int, List<Organism>> activeOrganismPairs = new Dictionary<int, List<Organism>>();
    [SerializeField] int startingOrganismAmount = 20;   
    [SerializeField] int currentGeneration = 0;
    int allPairsCount = 0;

    [Header("Counters")]
    [SerializeField] int allOrganismsCount = 0;
    [SerializeField] int activeOrganismsCount = 0;
    [SerializeField] int deadOrganismsCount = 0;

    [Header("Organism Types")]
    [SerializeField] int redTypeCount = 0;
    [SerializeField] int blueTypeCount = 0;
    [SerializeField] int purpleTypeCount = 0;

   

    [Header("Health")]
    [SerializeField] int setHealthAmount = 3;
    [SerializeField] Vector2Int randomHealthRange;

    [Header("Varients")]
    [SerializeField] SelectionType healthType = SelectionType.set;
    [SerializeField] SelectionType organismType = SelectionType.set;
    [SerializeField] int amountOfOffspringToProduce = 1;

    void Awake(){

        activeOrganisms[currentGeneration] = new List<Organism>(); 
        availableOrganisms[currentGeneration] = new List<Organism>();

        for (int i = 0; i < startingOrganismAmount; i++){

            CreateOrganism(i);
        }
    }

    void Update(){    

        if (Input.GetKeyDown(KeyCode.Space)){

            currentGeneration++;       
            DecreaseHealth();
            FindPartners();
            ProduceOffsprings();
        }
    }

    void DecreaseHealth(){

        // Key = generation, Value = List of organisms in generation
        foreach (KeyValuePair<int, List<Organism>> thisGeneration in activeOrganisms){

            for (int i = 0; i < thisGeneration.Value.Count; i++){

                thisGeneration.Value[i].health--;

                if (thisGeneration.Value[i].health <= 0){

                    OrganismDied(thisGeneration.Value[i]);
                }
            }
        }
    }

    void FindPartners(){

        // Loop through all generations
        foreach (KeyValuePair<int, List<Organism>> thisGeneration in activeOrganisms){         

            // Loop through all organisms in generation
            foreach (Organism org in thisGeneration.Value){                           

                // Only runs if current organism has no partner assigned
                if (org.assignedPartner == null){

                    // Assigns Partner1 
                    Organism current = org;

                    int timesRan = 0;

                    // Continues running until a partner is found
                    bool check = true;
                    while (check){

                        timesRan++;

                        // Ends loop if there are no available partners
                        if (timesRan < thisGeneration.Value.Count * 4){

                            // Generate random number within availableOrganisms Range
                            Organism partnerPotential = availableOrganisms[thisGeneration.Key][Random.Range(0, availableOrganisms[thisGeneration.Key].Count)];

                            // If the generated number is not in the current organisms family history and the random organism does not have an assigned partner
                            if (partnerPotential.assignedPartner == null && current.name != partnerPotential.name){

                                bool familyHistoryStatus = true;
                                foreach (Organism currentFamilyMember in current.familyHistory){

                                    // In familyHistory
                                    if (partnerPotential.familyHistory.Contains(currentFamilyMember)){

                                        familyHistoryStatus = false;
                                        Debug.Log("FAMILY HISTORY MATCH: " + currentFamilyMember.name + " WITH: " + current.name + " AND " + partnerPotential.name);
                                        break;

                                    }
                                }

                                if (familyHistoryStatus){

                                    AssignPartners(current, partnerPotential);
                                    check = false;
                                }                               
                            }
                            else
                            {
                                Debug.Log("First Fail  Partner: " + partnerPotential.name + " Current: "+ current.name);
                            }
                        }
                        else{
                            Debug.Log("TIMES RAN TOO HIGH");
                            check = false;
                        }
                    }
                }
            }
        }
    }   

    void OrganismDied(Organism organism){

        // Checks to see if dead organism has an assigned partner
        if (organism.assignedPartner != null){

            // Dead organisms partner
            Organism partner = organism.assignedPartner;

            // UnAssignes partnership between dead organism and their assigned partner
            partner.assignedPartner = null;

            activeOrganismPairs.Remove(organism.pairNum);
            availableOrganisms[partner.generation].Add(partner);
            partner.pairNum = 0;

            Debug.Log(organism.name + " DIED, LEAVING " + partner.name + " WITH NO PARTNER");
        }
        else{

            Debug.Log(organism.name + " DIED ALONE");
        }

        if(organism.type == "Red"){

            redTypeCount--;
        }
        else if (organism.type == "Blue"){

            blueTypeCount--;
        }
        else if (organism.type == "Purple"){

            purpleTypeCount--;
        }


        // Inactivates Organism
        activeOrganisms[organism.generation].Remove(organism);

        activeOrganismsCount--;
        deadOrganismsCount++;
    }

    void AssignPartners(Organism partner1, Organism partner2){

        // Assigns PartnerShip between both Organisms
        partner1.assignedPartner = partner2;
        partner2.assignedPartner = partner1;

        Debug.Log(partner1.name + " PARTNERED WITH " + partner2.name);

        allPairsCount++;
        activeOrganismPairs[allPairsCount] = new List<Organism> { partner1, partner2 };

        availableOrganisms[partner1.generation].Remove(partner1);
        availableOrganisms[partner1.generation].Remove(partner2);

        partner1.pairNum = allPairsCount;
        partner2.pairNum = allPairsCount;
    }

    // Creates Organisms for first Generation
    void CreateOrganism(int firstGenerationCount = 0){

        Organism newOrganism = new Organism();
        allOrganismsCount++;
        activeOrganismsCount++;
        newOrganism.generation = currentGeneration;

        // Set Organisms name 
        newOrganism.name = "Organism_" + allOrganismsCount;

        // Random Health
        if (healthType == SelectionType.random){

            newOrganism.health = Random.Range(randomHealthRange.x, randomHealthRange.y);
        }

        // Set Health
        else if (healthType == SelectionType.set){

            newOrganism.health = setHealthAmount;
        }


        // Random Type
        if (organismType == SelectionType.random){

            int typeChoice = Random.Range(0, 2);
            if (typeChoice == 0){

                newOrganism.type = "Red";
            }
            else if (typeChoice == 1){

                newOrganism.type = "Blue";
            }
        }

        // Set Type
        else if (organismType == SelectionType.set){

            // Half the organisms are assigned RED and the other half BLUE
             if (firstGenerationCount >= startingOrganismAmount / 2){

                newOrganism.type = "Red";
             }
             else if (firstGenerationCount < startingOrganismAmount / 2){

                newOrganism.type = "Blue";
             }
        }

        if (newOrganism.type == "Red"){

            redTypeCount++;
        }
        else if (newOrganism.type == "Blue"){

            blueTypeCount++;
        }
        else if (newOrganism.type == "Purple"){

            purpleTypeCount++;
        }

        // Set Family Tree
        newOrganism.familyHistory.Add(newOrganism);

        activeOrganisms[currentGeneration].Add(newOrganism);
        availableOrganisms[currentGeneration].Add(newOrganism);
    }

    // Creates Organisms for future Generation
    void CreateOrganism(Organism parent1, Organism parent2){

        Organism offspring = new Organism();
        allOrganismsCount++;
        activeOrganismsCount++;
        offspring.generation = currentGeneration;

        // Set Offsprings name 
        offspring.name = "Organism_" + allOrganismsCount;

        // Random Health
        if (healthType == SelectionType.random){

            offspring.health = Random.Range(randomHealthRange.x, randomHealthRange.y);
        }

        // Set Health
        else if (healthType == SelectionType.set){

            offspring.health = setHealthAmount;
        }

        // Set offspring Type
        offspring.type = SetOffspringType(parent1.type, parent2.type);
        if(offspring.type == "Red"){

            redTypeCount++;
        }
        else if(offspring.type == "Blue"){

            blueTypeCount++;
        }
        else if(offspring.type == "Purple"){

            purpleTypeCount++;
        }

        // Set Family Tree
        offspring.familyHistory.AddRange(parent1.familyHistory);
        offspring.familyHistory.AddRange(parent2.familyHistory);
        offspring.familyHistory.Add(offspring);


        activeOrganisms[offspring.generation].Add(offspring);
        availableOrganisms[offspring.generation].Add(offspring);
    }

    void ProduceOffsprings(){

        activeOrganisms[currentGeneration] = new List<Organism>();
        availableOrganisms[currentGeneration] = new List<Organism>();

        foreach (KeyValuePair<int, List<Organism>> organismPair in activeOrganismPairs){

            for (int i = 0; i < amountOfOffspringToProduce; i++){

                Debug.Log(organismPair.Value[0].name + " HAD AN OFFSPRING WITH " + organismPair.Value[1].name);
                CreateOrganism(organismPair.Value[0], organismPair.Value[1]);
            }
        }
    }

    string SetOffspringType(string parentOneType, string parentTwoType){

        switch (parentOneType){

            case "Red":
                
                switch (parentTwoType){

                    case "Red":

                        return "Red";

                    case "Blue":

                        return "Purple";

                    case "Purple":

                        return "Red";

                    default: return "NONE";


                }

            case "Blue":

                switch (parentTwoType){

                    case "Blue":

                        return "Blue";

                    case "Red":

                        return "Purple";

                    case "Purple":

                        return "Blue";

                    default: return "NONE";


                }

            case "Purple":

                switch (parentTwoType){

                    case "Purple":

                        return "Purple";

                    case "Red":

                        return "Red";

                    case "Blue":

                        return "Blue";

                    default: return "NONE";


                }

            default: return "NONE";
        }
    }
}

