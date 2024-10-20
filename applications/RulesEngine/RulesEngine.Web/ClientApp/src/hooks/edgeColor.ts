
const edgeColor = (edge: { relationship?: string | undefined, name?: string | undefined, substance?: string | undefined }) => {

    switch (edge.relationship) {
        case 'includedIn': return 'purple';
        default: break;
    }

    switch (edge.name) {
        case 'includedIn': return '#4E808F';
        case 'locatedIn': return '#8c8';
        case 'isCapabilityOf': return '#838';
        case 'isMeasureOf': return '#388';
        default: break;
    }

    switch (edge.substance) {
        case 'OutsideAir': return 'cyan';
        case 'SupplyAir': return 'cyan';
        case 'HotWater': return 'red';
        case 'FuelOil': return 'gold';
        case 'NaturalGas': return 'yellow';
        case 'DriveElec': return 'gold';
        case 'Condensate': return 'darkblue';
        case 'StormDrainage': return 'darkblue';
        case 'SprinklerWater': return 'darkblue';
        case 'ACElec': return 'orange';
        case 'ChilledWater': return 'blue';
        case 'MakeupWater': return 'blue';
        case 'ColdWater': return 'blue';
        case 'Water': return '#40E0D0';
    }

    return 'grey';
};

export default edgeColor;
