import { Apartment, Assignment, BadgeRounded, Group, Home, Key, Person, Shield } from "@mui/icons-material";
import SecurityIcon from '@mui/icons-material/Security';
import InfoIcon from '@mui/icons-material/Info';
import { Icon } from "@willowinc/ui";

export class AppIcons {
  static HomeIcon = <Icon icon="home" />;
  static UserIcons = <Icon icon="person" />;
  static GroupIcon = <Icon icon="group" />;
  static RoleIcon = <BadgeRounded />;
  static PermissionIcon = <Key />;
  static AssignmentIcon = <Assignment />;
  static AboutIcon = <InfoIcon />;
  static AdminIcon = <Shield />;
  static ApartmentIcon = <Apartment />;
}
